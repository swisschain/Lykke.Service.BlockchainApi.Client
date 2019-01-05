using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Chaos;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.BlockchainApi.Sdk.Domain.Assets;
using Lykke.Service.BlockchainApi.Sdk.Domain.DepositWallets;
using Lykke.Service.BlockchainApi.Sdk.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/balances")]
    public class BalancesController : ControllerBase
    {
        readonly AssetRepository _assets;
        readonly DepositWalletRepository _depositWallets;
        readonly IBlockchainApi _api;
        readonly IChaosKitty _chaosKitty;

        public BalancesController(AssetRepository assets, DepositWalletRepository depositWallets, IBlockchainApi api, IChaosKitty chaosKitty = null)
        {
            _assets = assets;
            _depositWallets = depositWallets;
            _api = api;
            _chaosKitty = chaosKitty;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<WalletBalanceContract>>> Get(int take, string continuation) 
        {
            if (take <= 0)
            {
                return BadRequest("'take' must be grater than zero");
            }

            if (!AzureContinuationValidator.IsValid(continuation))
            {
                return BadRequest("'continuation' must be null or valid Azure continuation token");
            }

            IEnumerable<DepositWalletBalanceEntity> balances;

            if (_api.CanGetBalances)
            {
                // to be able to enroll fake transfers properly
                // we keep block number increased by one order of magnitude
                var lastConfirmedBlock = await _api.GetLastConfirmedBlockNumberAsync() * 10;
                var depositWallets = await _depositWallets.GetWalletsAsync(take, continuation);
                var addresses = depositWallets.items.Select(w => w.Address).ToArray();

                continuation = depositWallets.continuation;
                balances = (await _api.GetBalancesAsync(addresses, async assetId => await _assets.GetAsync(assetId)))
                    .Select(b => new DepositWalletBalanceEntity(b.Address, b.AssetId)
                    {
                        Amount = b.Amount,
                        BlockNumber = lastConfirmedBlock
                    });
            }
            else
            {
                (balances, continuation) = 
                    await _depositWallets.GetBalanceAsync(take, continuation);
            }
            
            var result = new List<WalletBalanceContract>();

            foreach (var balance in balances)
            {
                // blockchain may return unknown assets,
                // filter out such items

                var accuracy = (await _assets.GetAsync(balance.AssetId))?.Accuracy;
                if (accuracy == null)
                {
                    continue;
                }

                result.Add(new WalletBalanceContract
                {
                    Address = balance.Address,
                    AssetId = balance.AssetId,
                    Balance = Conversions.CoinsToContract(balance.Amount, accuracy.Value),
                    Block = balance.BlockNumber
                });
            }

            return PaginationResponse.From(continuation, result);
        }
    
        [HttpPost("{address}/observation")]
        public async Task<ActionResult> Observe(string address)
        {
            if (!_api.AddressIsValid(address))
            {
                return BadRequest("'address' must be valid blockchain address");
            }

            if (await _depositWallets.TryObserveAsync(address))
            {
                _chaosKitty?.Meow($"{nameof(Observe)}_Data");

                await _api.ObserveAddressAsync(address);

                _chaosKitty?.Meow($"{nameof(Observe)}_Blockchain");

                return Ok();
            }
            else
            {
                return Conflict($"Address {address} is already observed");
            }
        }

        [HttpDelete("{address}/observation")]
        public async Task<ActionResult> DeleteObservation(string address)
        {
            if (!_api.AddressIsValid(address))
            {
                return BadRequest("'address' must be valid blockchain address");
            }
            
            if (await _depositWallets.TryDeleteObservationAsync(address))
            {
                _chaosKitty?.Meow($"{nameof(DeleteObservation)}_Data");

                await _api.DeleteAddressObservationAsync(address);

                _chaosKitty?.Meow($"{nameof(DeleteObservation)}_Blockchain");

                return Ok();
            }
            else
            {
                return NoContent();
            }
        }
    }
}