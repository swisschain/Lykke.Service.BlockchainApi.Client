using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Common.Chaos;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.BlockchainApi.Sdk.Domain.Assets;
using Lykke.Service.BlockchainApi.Sdk.Domain.DepositWallets;
using Lykke.Service.BlockchainApi.Sdk.Domain.Operations;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/transactions")]
    public class TransactionsController : ControllerBase
    {
        readonly IBlockchainApi _api;
        readonly OperationRepository _operations;
        readonly AssetRepository _assets;
        readonly DepositWalletRepository _depositWallets;
        readonly IChaosKitty _chaosKitty;

        public TransactionsController(
            IBlockchainApi api, 
            OperationRepository operations,
            AssetRepository assets, 
            DepositWalletRepository depositWallets, 
            IChaosKitty chaosKitty = null)
        {
            _api = api;
            _operations = operations;
            _assets = assets;
            _depositWallets = depositWallets;
            _chaosKitty = chaosKitty;
        }

        private async Task<ActionResult<BuildTransactionResponse>> Build(
            Guid operationId, 
            string assetId, 
            (string from, string fromContext, string to, string amount)[] inOuts,
            bool includeFee)
        {
            var operation = await _operations.GetAsync(operationId);

            if (operation?.SendTime != null ||
                operation?.FailTime != null)
            {
                return Conflict($"Operation {operationId} is already {operation.GetState()}");
            }

            var asset = await _assets.GetAsync(assetId);

            if (asset == null)
            {
                return BadRequest(BlockchainErrorResponse.Create("Unknown asset"));
            }

            try
            {
                // convert string amounts to decimal and validate
                var actions = inOuts
                    .Select((a, i) => new OperationAction(i.ToString("D4"), a.from, a.fromContext, a.to, amount: Conversions.CoinsFromContract(a.amount, asset.Accuracy)))
                    .ToArray();

                if (actions.Any(a => a.Amount <= 0 || !_api.AddressIsValid(a.From) || !_api.AddressIsValid(a.To)))
                {
                    return BadRequest(BlockchainErrorResponse.Create("Invalid address(es) and/or negative amount(s)"));
                }

                var separator = _api.GetConstants().PublicAddressExtension?.Separator ?? char.MinValue;

                // fail if both action types are contained in the list,
                // or if the list is empty
                if (!(actions.Any(a => a.IsReal(separator)) ^ actions.Any(a => a.IsFake(separator))))
                {
                    return BadRequest(BlockchainErrorResponse.Create("Transfers must not be empty and must be either all fake or all real"));
                }

                var (tx, fee, expiration) = (Constants.DUMMY_TX, 0M, 0L);

                if (actions.Any(a => a.IsReal(separator)))
                {
                    (tx, fee, expiration) = await _api.BuildTransactionAsync(operationId, asset, actions, includeFee);
                }
                else
                {
                    foreach (var writeOff in actions.GroupBy(a => a.From).Select(g => new { Address = g.Key, Amount = g.Sum(a => a.Amount) }))
                    {
                        var balance = await _depositWallets.GetBalanceAsync(writeOff.Address, assetId);

                        if (balance == null ||
                            balance.Amount < writeOff.Amount)
                        {
                            return BadRequest(BlockchainErrorResponse.FromKnownError(BlockchainErrorCode.NotEnoughBalance));
                        }
                    }
                }

                await _operations.UpsertAsync(operationId, assetId, actions, includeFee, fee, expiration);

                _chaosKitty?.Meow(nameof(Build));

                return new BuildTransactionResponse { TransactionContext = tx.ToBase64() };
            }
            catch (ArgumentException ex)
            {
                // nonsense in request
                return BadRequest(BlockchainErrorResponse.Create(ex.ToString()));
            }
            catch (ConversionException ex)
            {
                return BadRequest(BlockchainErrorResponse.Create("Invalid amount(s)"));
            }
            catch (BlockchainException ex)
            {
                // meaningful error
                return BadRequest(BlockchainErrorResponse.FromKnownError(ex.ErrorCode));
            }
        }

        private async Task<ActionResult<TResponse>> UpdateOperationState<TResponse>(Guid operationId, Func<OperationEntity, AssetEntity, TResponse> toResponse) 
            where TResponse : BaseBroadcastedTransactionResponse
        {
            var operation = await _operations.GetAsync(operationId);

            // check if operation is already processed by common services
            if (operation == null || operation.DeleteTime != null)
            {
                return NoContent();
            }

            var asset = await _assets.GetAsync(operation.AssetId) ??
                throw new InvalidOperationException($"Asset {operation.AssetId} not found, possibly was deleted.");

            // check if transaction is already processed
            if (operation.FailTime != null || operation.CompletionTime != null)
            {
                return toResponse(operation, asset); 
            }

            var tx = await _api.GetTransactionAsync(operation.TransactionHash, operation.Expiration, asset);

            switch (tx.State)
            {
                case BroadcastedTransactionState.Completed:
                    // to be able to enroll fake transfers properly
                    // we keep block number increased by one order of magnitude
                    tx.BlockNumber *= 10;
                    tx.Actions.ForEach(a => a.BlockNumber = tx.BlockNumber.Value);
                    
                    await _depositWallets.EnrollIfObservedAsync(tx.Actions, operationId);

                    _chaosKitty?.Meow(nameof(UpdateOperationState));

                    return toResponse(
                        await _operations.UpdateAsync(operationId, completionTime: DateTime.UtcNow, blockTime: tx.BlockTime, blockNumber: tx.BlockNumber),
                        asset
                    );

                case BroadcastedTransactionState.Failed:
                    return toResponse(
                        await _operations.UpdateAsync(operationId, failTime: DateTime.UtcNow, error: tx.Error, errorCode: tx.ErrorCode),
                        asset
                    );

                default:
                    return toResponse(operation, asset);
            }
        }

        [HttpPost("single")]
        public async Task<ActionResult<BuildTransactionResponse>> BuildSingle(BuildSingleTransactionRequest request)
        {
            return await Build(
                request.OperationId, 
                request.AssetId, 
                new []{(request.FromAddress, request.FromAddressContext, request.ToAddress, request.Amount)},
                request.IncludeFee
            );
        }

        [HttpPost("single/receive")]
        [ProducesResponseType(501)]
        public ActionResult BuildSingleReceive(BuildSingleReceiveTransactionRequest request) => StatusCode(501);

        [HttpPost("many-inputs")]
        public async Task<ActionResult<BuildTransactionResponse>> BuildManyInputs(BuildTransactionWithManyInputsRequest request)
        {
            return await Build(
                request.OperationId, 
                request.AssetId, 
                request.Inputs.Select(x => (x.FromAddress, x.FromAddressContext, request.ToAddress, x.Amount)).ToArray(),
                true
            );
        }

        [HttpPost("many-outputs")]
        public async Task<ActionResult<BuildTransactionResponse>> BuildManyOutputs(BuildTransactionWithManyOutputsRequest request)
        {
            return await Build(
                request.OperationId,
                request.AssetId,
                request.Outputs.Select(x => (request.FromAddress, request.FromAddressContext, x.ToAddress, x.Amount)).ToArray(),
                false
            );
        }

        [HttpPut]
        [ProducesResponseType(501)]
        public ActionResult Rebuild(RebuildTransactionRequest request) => StatusCode(501);

        [HttpPost("broadcast")]
        public async Task<ActionResult<string>> Broadcast(BroadcastTransactionRequest request)
        {
            var operation = await _operations.GetAsync(request.OperationId);
            if (operation == null)
            {
                return BadRequest(BlockchainErrorResponse.Create("Unknown operation"));
            }
            else if (operation.FailTime != null)
            {
                if (operation.ErrorCode == null)
                    return BadRequest(BlockchainErrorResponse.Create(operation.Error));
                else
                    return BadRequest(BlockchainErrorResponse.FromKnownError(operation.ErrorCode.Value));
            }
            else if (operation.SendTime != null || operation.CompletionTime != null)
            {
                return Conflict($"Operation {request.OperationId} is already {operation.GetState()}");
            }

            var (hash, signedTransaction) = JsonConvert.DeserializeObject<(string, string)>(request.SignedTransaction.Base64ToString());

            // prevent collisions
            var operationIndex = await _operations.GetOperationIndexAsync(hash);
            if (operationIndex != null && 
                operationIndex.OperationId != operation.OperationId)
            {
                var error = "Duplicated transaction hash";
                var errorCode = BlockchainErrorCode.BuildingShouldBeRepeated;

                await _operations.UpdateAsync(operation.OperationId, failTime: DateTime.UtcNow, error: error, errorCode: errorCode);

                _chaosKitty?.Meow($"{nameof(Broadcast)}_DuplicatedHash)");

                return BadRequest(BlockchainErrorResponse.FromKnownError(errorCode));
            }

            if (signedTransaction == Constants.DUMMY_TX)
            {
                // for fully simulated transaction we must immediately
                // enroll balances to prevent double spent
                
                // to be able to enroll fake transfers properly
                // we keep block number increased by one order of magnitude,
                // we increase adjusted block number to distinguish fake and real transfers

                var now = DateTime.UtcNow;
                var blockNumber = await _api.GetLastConfirmedBlockNumberAsync() * 10 + 1;
                var actions = operation.Actions.SelectMany(a => new []
                {
                    new BlockchainAction(a.ActionId, blockNumber, now, hash, a.From, operation.AssetId, (-1) * (a.Amount)),
                    new BlockchainAction(a.ActionId, blockNumber, now, hash, a.To, operation.AssetId, a.Amount),
                });

                await _depositWallets.EnrollIfObservedAsync(actions, operation.OperationId);

                _chaosKitty?.Meow($"{nameof(Broadcast)}_Fake_Enroll)");

                await _operations.UpdateAsync(operation.OperationId, transactionHash: hash, 
                    sendTime: now, completionTime: now, blockTime: now, blockNumber: blockNumber);

                _chaosKitty?.Meow($"{nameof(Broadcast)}_Fake_Update)");

                return Ok(hash);
            }

            try
            {
                var result = await _api.BroadcastTransactionAsync(signedTransaction);

                _chaosKitty?.Meow($"{nameof(Broadcast)}_Real_Broadcast)");

                await _operations.UpdateAsync(operation.OperationId, transactionHash: hash, 
                    sendTime: DateTime.UtcNow, broadcastResult: result);

                _chaosKitty?.Meow($"{nameof(Broadcast)}_Real_Update)");

                return Ok(hash);
            }
            catch (ArgumentException ex)
            {
                // nonsense in request
                return BadRequest(BlockchainErrorResponse.Create(ex.ToString()));
            }
            catch (BlockchainException ex) when (ex.ErrorCode == BlockchainErrorCode.AmountIsTooSmall || ex.ErrorCode == BlockchainErrorCode.BuildingShouldBeRepeated)
            {
                // save state to prevent double-sending
                await _operations.UpdateAsync(operation.OperationId, transactionHash: hash,
                    failTime: DateTime.UtcNow, error: ex.Message, errorCode: ex.ErrorCode);

                _chaosKitty?.Meow($"{nameof(Broadcast)}_Real_Fail)");

                return BadRequest(BlockchainErrorResponse.FromKnownError(ex.ErrorCode));
            }
            catch (BlockchainException ex)
            {
                // meaningful, possibly recoverable error
                return BadRequest(BlockchainErrorResponse.FromKnownError(ex.ErrorCode));
            }
        }

        [HttpGet("broadcast/single/{operationId}")]
        public async Task<ActionResult<BroadcastedSingleTransactionResponse>> GetSingle(Guid operationId)
        {
            return await UpdateOperationState(
                operationId, 
                (op, asset)  => new BroadcastedSingleTransactionResponse
                {
                    OperationId = operationId,
                    Amount = Conversions.CoinsToContract(op.Amount, asset.Accuracy),
                    Fee = Conversions.CoinsToContract(op.Fee, asset.Accuracy),
                    Block = op.BlockNumber ?? 0L,
                    Error = op.Error,
                    ErrorCode = op.ErrorCode,
                    Hash = op.TransactionHash,
                    State = op.GetState(),
                    Timestamp = op.GetTimestamp()
                }
            );
        }

        [HttpGet("broadcast/many-inputs/{operationId}")]
        public async Task<ActionResult<BroadcastedTransactionWithManyInputsResponse>> GetManyInputs([FromRoute]Guid operationId)
        {
            return await UpdateOperationState(
                operationId,
                (op, asset) => new BroadcastedTransactionWithManyInputsResponse
                {
                    OperationId = operationId,
                    Fee = Conversions.CoinsToContract(op.Fee, asset.Accuracy),
                    Block = op.BlockNumber ?? 0L,
                    Error = op.Error,
                    ErrorCode = op.ErrorCode,
                    Hash = op.TransactionHash,
                    State = op.GetState(),
                    Timestamp = op.GetTimestamp(),
                    Inputs = op.Actions
                        .Select(a => new BroadcastedTransactionInputContract { FromAddress = a.From, Amount = Conversions.CoinsToContract(a.Amount, asset.Accuracy) })
                        .ToArray()
                }
            );
        }

        [HttpGet("broadcast/many-outputs/{operationId}")]
        public async Task<ActionResult<BroadcastedTransactionWithManyOutputsResponse>> GetManyOutputs(Guid operationId)
        {
            return await UpdateOperationState(
                operationId,
                (op, asset) => new BroadcastedTransactionWithManyOutputsResponse
                {
                    OperationId = operationId,
                    Fee = Conversions.CoinsToContract(op.Fee, asset.Accuracy),
                    Block = op.BlockNumber ?? 0L,
                    Error = op.Error,
                    ErrorCode = op.ErrorCode,
                    Hash = op.TransactionHash,
                    State = op.GetState(),
                    Timestamp = op.GetTimestamp(),
                    Outputs = op.Actions
                        .Select(a => new BroadcastedTransactionOutputContract { ToAddress = a.To, Amount = Conversions.CoinsToContract(a.Amount, asset.Accuracy) })
                        .ToArray()
                }
            );
        }

        [HttpDelete("broadcast/{operationId}")]
        public async Task<ActionResult> DeleteBroadcasted(Guid operationId)
        {
            var operation = await _operations.GetAsync(operationId);
            if (operation != null && 
                operation.DeleteTime == null)
            {
                await _operations.UpdateAsync(operationId, deleteTime: DateTime.UtcNow);

                _chaosKitty?.Meow(nameof(DeleteBroadcasted));

                return Ok();
            }
            else
                return NoContent();
        }

        [HttpPost("history/from/{address}/observation")]
        [ProducesResponseType(501)]
        public ActionResult ObserveFrom(string address) => StatusCode(501);

        [HttpPost("history/to/{address}/observation")]
        [ProducesResponseType(501)]
        public ActionResult ObserveTo(string address) => StatusCode(501);

        [HttpDelete("history/from/{address}/observation")]
        [ProducesResponseType(501)]
        public ActionResult DeleteObservationFrom(string address) => StatusCode(501);

        [HttpDelete("history/to/{address}/observation")]
        [ProducesResponseType(501)]
        public ActionResult DeleteObservationTo(string address) => StatusCode(501);

        [HttpGet("history/from/{address}")]
        [ProducesResponseType(501)]
        public ActionResult GetHistoryFrom(string address, string afterHash, int take) => StatusCode(501);

        [HttpGet("history/to/{address}")]
        [ProducesResponseType(501)]
        public ActionResult GetHistoryTo(string address, string afterHash, int take) => StatusCode(501);
    }
}