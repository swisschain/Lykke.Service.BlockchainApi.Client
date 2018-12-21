using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Common;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class BlockchainApiClient : IBlockchainApiClient
    {
        public string HostUrl { get; private set; }

        private HttpClient _httpClient;
        private IBlockchainApi _api;
        private ApiRunner _runner;

        /// <param name="timeout">Timeout affects all request. Leave it null if you do not want timeout.</param>
        private void InitBlockchainApiClient(ILog log, string hostUrl, int retriesCount = 5,
            TimeSpan? timeout = null, HttpMessageHandler messageHandler = null)
        {
            HostUrl = hostUrl ?? throw new ArgumentNullException(nameof(hostUrl));

            _httpClient = new HttpClient(new HttpErrorLoggingHandler(log, messageHandler))
            {
                BaseAddress = new Uri(hostUrl),
                DefaultRequestHeaders =
                {
                    {
                        "User-Agent",
                        $"{PlatformServices.Default.Application.ApplicationName}/{PlatformServices.Default.Application.ApplicationVersion}"
                    }
                }
            };

            if (timeout.HasValue)
            {
                _httpClient.Timeout = timeout.Value;
            }
            

            _api = RestService.For<IBlockchainApi>(_httpClient);

            _runner = new ApiRunner(retriesCount);
        }

        [Obsolete("Please, use the overload which consumes ILogFactory.")]
        public BlockchainApiClient(ILog log, string hostUrl, int retriesCount = 5)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            InitBlockchainApiClient(log, hostUrl, retriesCount,  null, null);
        }

        public BlockchainApiClient(ILogFactory logFactory, string hostUrl, int retriesCount = 5)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            InitBlockchainApiClient(logFactory.CreateLog(this), hostUrl, retriesCount, null, null);
        }

        /// <summary>
        /// With timeout parameter
        /// </summary>
        /// <param name="timeout">If operation takes more time than stated in timeout variable,
        /// <exception cref="TaskCanceledException ">TaskCanceledException </exception>will be thrown</param>
        public BlockchainApiClient(ILogFactory logFactory, string hostUrl, int retriesCount = 5, TimeSpan? timeout = null)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            InitBlockchainApiClient(logFactory.CreateLog(this), hostUrl, retriesCount, timeout, null);
        }

        internal BlockchainApiClient(ILogFactory logFactory, string hostUrl, int retriesCount = 5, 
            TimeSpan? timeout = null, HttpMessageHandler handler = null)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            InitBlockchainApiClient(logFactory.CreateLog(this), hostUrl, retriesCount, timeout, handler);
        }


        #region General

        /// <inheritdoc />
        public Task<IsAliveResponse> GetIsAliveAsync()
        {
            return _runner.RunAsync(() => _api.GetIsAliveAsync());
        }

        /// <inheritdoc />
        public async Task<BlockchainCapabilities> GetCapabilitiesAsync()
        {
            var response = await _runner.RunAsync(() => _api.GetCapabilitiesAsync());

            return new BlockchainCapabilities(response);
        }

        /// <inheritdoc />
        public async Task<BlockchainConstants> GetConstantsAsync()
        {
            try
            {
                var response = await _runner.RunAsync(() => _api.GetConstantsAsync());

                return new BlockchainConstants(response);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotImplemented || ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new BlockchainConstants(new ConstantsResponse());
            }
        }

        #endregion


        #region Assets

        /// <inheritdoc />
        public Task<EnumerationStatistics> EnumerateAllAssetsAsync(int batchSize, Action<BlockchainAsset> enumerationCallback)
        {
            return EnumerateAllAssetBatchesAsync(batchSize, batch =>
            {
                foreach (var asset in batch)
                {
                    enumerationCallback(asset);
                }
            });
        }

        /// <inheritdoc />
        public Task<EnumerationStatistics> EnumerateAllAssetsAsync(int batchSize, Func<BlockchainAsset, Task> enumerationCallback)
        {
            return EnumerateAllAssetBatchesAsync(batchSize, async batch =>
            {
                foreach (var asset in batch)
                {
                    await enumerationCallback(asset);
                }
            });
        }

        /// <inheritdoc />
        public async Task<EnumerationStatistics> EnumerateAllAssetBatchesAsync(int batchSize, Action<IReadOnlyList<BlockchainAsset>> enumerationCallback)
        {
            var statisticsBuilder = new EnumerationStatisticsBuilder();
            string continuation = null;

            do
            {
                var response = await GetAssetsAsync(batchSize, continuation);

                statisticsBuilder.IncludeBatch(response.Items);

                enumerationCallback(response.Items);

                continuation = response.Continuation;

                if (!response.HasMoreItems)
                {
                    return statisticsBuilder.Build();
                }

            } while (true);
        }

        /// <inheritdoc />
        public async Task<EnumerationStatistics> EnumerateAllAssetBatchesAsync(int batchSize, Func<IReadOnlyList<BlockchainAsset>, Task> enumerationCallback)
        {
            var statisticsBuilder = new EnumerationStatisticsBuilder();
            string continuation = null;

            do
            {
                var response = await GetAssetsAsync(batchSize, continuation);

                statisticsBuilder.IncludeBatch(response.Items);

                await enumerationCallback(response.Items);

                continuation = response.Continuation;

                if (!response.HasMoreItems)
                {
                    return statisticsBuilder.Build();
                }

            } while (true);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<string, BlockchainAsset>> GetAllAssetsAsync(int batchSize)
        {
            var result = new Dictionary<string, BlockchainAsset>();

            await EnumerateAllAssetsAsync(batchSize, asset => result[asset.AssetId] = asset);

            return result;
        }

        /// <inheritdoc />
        public async Task<BlockchainAsset> GetAssetAsync(string assetId)
        {
            var asset = await TryGetAssetAsync(assetId);

            if (asset == null)
            {
                throw new ResultValidationException("Asset not found");
            }

            return asset;
        }

        public async Task<BlockchainAsset> TryGetAssetAsync(string assetId)
        {
            ValidateAssetIdIsNotEmpty(assetId);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetAssetAsync(assetId));

            if (apiResponse == null)
            {
                return null;
            }

            return new BlockchainAsset(apiResponse);
        }

        #endregion


        #region Addresses

        /// <inheritdoc />
        public async Task<bool> IsAddressValidAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            return (await _runner.RunWithRetriesAsync(() => _api.IsAddressValidAsync(address))).IsValid;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Uri>> GetAddressExplorerUrlAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            var result = await _runner.RunWithRetriesAsync(() => _api.GetAddressExplorerUrlsAsync(address));

            return result != null
                ? result
                    .Select(u => new Uri(u))
                    .ToArray()
                : Array.Empty<Uri>();
        }

        /// <inheritdoc />
        public async Task<string> GetUnderlyingAddressAsync(string virtualAddress)
        {
            ValidateAddressIsNotEmpty(virtualAddress);

            return (await _runner.RunWithRetriesAsync(() => _api.GetUnderlyingAddressAsync(virtualAddress))).UnderlyingAddress;
        }

        /// <inheritdoc />
        public async Task<string> GetVirtualAddressAsync(string underlyingAddress)
        {
            ValidateAddressIsNotEmpty(underlyingAddress);

            return (await _runner.RunWithRetriesAsync(() => _api.GetVirtualAddressAsync(underlyingAddress))).VirtualAddress;
        }

        #endregion


        #region Balances

        /// <inheritdoc />
        public async Task<bool> StartBalanceObservationAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            try
            {
                await _runner.RunWithRetriesAsync(() => _api.StartBalanceObservationAsync(address));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> StopBalanceObservationAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            try
            {
                await _runner.RunWithRetriesAsync(() => _api.StopBalanceObservationAsync(address));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<EnumerationStatistics> EnumerateWalletBalanceBatchesAsync(int batchSize, Func<string, int> assetAccuracyProvider, Func<IReadOnlyList<WalletBalance>, Task<bool>> enumerationCallback)
        {
            var statisticsBuilder = new EnumerationStatisticsBuilder();
            string continuation = null;

            do
            {
                var response = await GetWalletBalancesAsync(batchSize, continuation, assetAccuracyProvider);

                statisticsBuilder.IncludeBatch(response.Items);

                await enumerationCallback(response.Items);

                continuation = response.Continuation;

                if (!response.HasMoreItems)
                {
                    return statisticsBuilder.Build();
                }

            } while (true);
        }

        #endregion


        #region Transactions building

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> BuildSingleTransactionAsync(Guid operationId, string fromAddress, string fromAddressContext, string toAddress, BlockchainAsset asset, decimal amount, bool includeFee)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateFromAddresIsNotEmpty(fromAddress);
            ValidateToAddressIsNotEmpty(toAddress);
            ValidateAssetIsNotNull(asset);
            ValidateAmountRange(amount);

            try
            {
                var apiResponse = await _runner.RunWithRetriesAsync(() => _api.BuildSingleTransactionAsync(
                    new BuildSingleTransactionRequest
                    {
                        OperationId = operationId,
                        FromAddress = fromAddress,
                        FromAddressContext = fromAddressContext,
                        ToAddress = toAddress,
                        AssetId = asset.AssetId,
                        Amount = Conversions.CoinsToContract(amount, asset.Accuracy),
                        IncludeFee = includeFee
                    }));

                return new TransactionBuildingResult(apiResponse);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new TransactionAlreadyBroadcastedException(ex);
            }
        }

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> BuildSingleReceiveTransactionAsync(Guid operationId, string sendTransactionHash)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateSendTransactionHashIsNotEmpty(sendTransactionHash);

            try
            {
                var apiResponse = await _runner.RunWithRetriesAsync(() => _api.BuildSingleReceiveTransactionAsync(
                    new BuildSingleReceiveTransactionRequest
                    {
                        OperationId = operationId,
                        SendTransactionHash = sendTransactionHash
                    }));

                return new TransactionBuildingResult(apiResponse);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotImplemented)
            {
                throw new NotSupportedException("Operation is not supported by the blockchain. See GetCapabilitiesAsync", ex);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new TransactionAlreadyBroadcastedException(ex);
            }
        }

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> BuildTransactionWithManyInputsAsync(Guid operationId, IEnumerable<BuildingTransactionInput> inputs, string toAddress, BlockchainAsset asset)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            // ReSharper disable once PossibleMultipleEnumeration
            ValidateInputsNotNull(inputs);
            ValidateToAddressIsNotEmpty(toAddress);
            ValidateAssetIsNotNull(asset);

            try
            {
                var apiResponse = await _runner.RunWithRetriesAsync(() => _api.BuildTransactionWithManyInputsAsync(
                    new BuildTransactionWithManyInputsRequest
                    {
                        OperationId = operationId,
                        // ReSharper disable once PossibleMultipleEnumeration
                        Inputs = inputs
                            .Select(i => i.ToContract(asset.Accuracy))
                            .ToArray(),
                        ToAddress = toAddress,
                        AssetId = asset.AssetId
                    }));

                return new TransactionBuildingResult(apiResponse);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotImplemented)
            {
                throw new NotSupportedException("Operation is not supported by the blockchain. See GetCapabilitiesAsync", ex);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new TransactionAlreadyBroadcastedException(ex);
            }
        }

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> BuildTransactionWithManyOutputsAsync(Guid operationId, string fromAddress, string fromAddressContext, IEnumerable<BuildingTransactionOutput> outputs, BlockchainAsset asset)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateToAddressIsNotEmpty(fromAddress);
            // ReSharper disable once PossibleMultipleEnumeration
            ValidateOutputsNotNull(outputs);
            ValidateAssetIsNotNull(asset);

            try
            {
                var apiResponse = await _runner.RunWithRetriesAsync(() => _api.BuildTransactionWithManyOutputsAsync(
                    new BuildTransactionWithManyOutputsRequest
                    {
                        OperationId = operationId,
                        FromAddress = fromAddress,
                        FromAddressContext = fromAddressContext,
                        // ReSharper disable once PossibleMultipleEnumeration
                        Outputs = outputs
                            .Select(o => o.ToContract(asset.Accuracy))
                            .ToArray(),
                        AssetId = asset.AssetId
                    }));

                return new TransactionBuildingResult(apiResponse);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotImplemented)
            {
                throw new NotSupportedException("Operation is not supported by the blockchain. See GetCapabilitiesAsync", ex);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new TransactionAlreadyBroadcastedException(ex);
            }
        }

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> RebuildTransactionAsync(
            Guid operationId,
            decimal feeFactor)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateFeeFactorRange(feeFactor);

            try
            {
                var apiResponse = await _runner.RunWithRetriesAsync(() => _api.RebuildTransactionAsync(
                    new RebuildTransactionRequest
                    {
                        OperationId = operationId,
                        FeeFactor = feeFactor
                    }));

                return new TransactionBuildingResult(apiResponse);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotImplemented)
            {
                throw new NotSupportedException("Operation is not supported by the blockchain. See GetCapabilitiesAsync", ex);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new TransactionAlreadyBroadcastedException(ex);
            }
        }

        #endregion


        #region Transactions broadcasting

        /// <inheritdoc />
        public async Task<TransactionBroadcastingResult> BroadcastTransactionAsync(Guid operationId, string signedTransaction)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateSignedTransactionIsNotEmpty(signedTransaction);

            try
            {
                await _runner.RunWithRetriesAsync(() => _api.BroadcastTransactionAsync(new BroadcastTransactionRequest
                {
                    OperationId = operationId,
                    SignedTransaction = signedTransaction
                }));

                return TransactionBroadcastingResult.Success;
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.BadRequest && ex.ErrorCode != BlockchainErrorCode.Unknown)
            {
                return TransactionBroadcastingResultMapper.FromErrorCode(ex.ErrorCode);
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return TransactionBroadcastingResult.AlreadyBroadcasted;
            }
        }

        /// <inheritdoc />
        public async Task<BroadcastedSingleTransaction> TryGetBroadcastedSingleTransactionAsync(Guid operationId, BlockchainAsset asset)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateAssetIsNotNull(asset);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetBroadcastedSingleTransactionAsync(operationId));

            return apiResponse == null
                ? null
                : new BroadcastedSingleTransaction(apiResponse, asset.Accuracy, operationId);
        }

        /// <inheritdoc />
        public async Task<BroadcastedSingleTransaction> GetBroadcastedSingleTransactionAsync(Guid operationId, BlockchainAsset asset)
        {
            var result = await TryGetBroadcastedSingleTransactionAsync(operationId, asset);

            if (result == null)
            {
                throw new ResultValidationException("Transaction is not found");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<BroadcastedTransactionWithManyInputs> TryGetBroadcastedTransactionWithManyInputsAsync(Guid operationId, BlockchainAsset asset)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateAssetIsNotNull(asset);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetBroadcastedTransactionWithManyInputsAsync(operationId));

            return apiResponse == null
                ? null
                : new BroadcastedTransactionWithManyInputs(apiResponse, asset.Accuracy, operationId);
        }

        /// <inheritdoc />
        public async Task<BroadcastedTransactionWithManyInputs> GetBroadcastedTransactionWithManyInputsAsync(Guid operationId, BlockchainAsset asset)
        {
            var result = await TryGetBroadcastedTransactionWithManyInputsAsync(operationId, asset);

            if (result == null)
            {
                throw new ResultValidationException("Transaction is not found");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<BroadcastedTransactionWithManyOutputs> TryGetBroadcastedTransactionWithManyOutputsAsync(Guid operationId, BlockchainAsset asset)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateAssetIsNotNull(asset);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetBroadcastedTransactionWithManyOutputsAsync(operationId));

            return apiResponse == null
                ? null
                : new BroadcastedTransactionWithManyOutputs(apiResponse, asset.Accuracy, operationId);
        }

        /// <inheritdoc />
        public async Task<BroadcastedTransactionWithManyOutputs> GetBroadcastedTransactionWithManyOutputsAsync(Guid operationId, BlockchainAsset asset)
        {
            var result = await TryGetBroadcastedTransactionWithManyOutputsAsync(operationId, asset);

            if (result == null)
            {
                throw new ResultValidationException("Transaction is not found");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> ForgetBroadcastedTransactionsAsync(Guid operationId)
        {
            try
            {
                ValidateOperationIdIsNotEmpty(operationId);

                await _runner.RunWithRetriesAsync(() => _api.ForgetBroadcastedTransactionAsync(operationId));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
            {
                return false;
            }

            return true;
        }

        #endregion


        #region Transactions history
        
        /// <inheritdoc />
        public async Task<bool> StartHistoryObservationOfOutgoingTransactionsAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            try
            {
                await _runner.RunWithRetriesAsync(() => _api.StartHistoryObservationOfOutgoingTransactionsAsync(address));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> StartHistoryObservationOfIncomingTransactionsAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            try
            {
                await _runner.RunWithRetriesAsync(() => _api.StartHistoryObservationOfIncomingTransactionsAsync(address));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HistoricalTransaction>> GetHistoryOfOutgoingTransactionsAsync(
            string address, 
            string afterHash, 
            int take,
            Func<string, int> assetAccuracyProvider)
        {
            ValidateAddressIsNotEmpty(address);
            ValidateTakeRange(take);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetHistoryOfOutgoingTransactionsAsync(address, afterHash, take));

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(t => new HistoricalTransaction(t, assetAccuracyProvider(t.AssetId)));
        }       

        /// <inheritdoc />
        public async Task<IEnumerable<HistoricalTransaction>> GetHistoryOfIncomingTransactionsAsync(
            string address,
            string afterHash,
            int take,
            Func<string, int> assetAccuracyProvider)
        {
            ValidateAddressIsNotEmpty(address);
            ValidateTakeRange(take);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetHistoryOfIncomingTransactionsAsync(address, afterHash, take));

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(t => new HistoricalTransaction(t, assetAccuracyProvider(t.AssetId)));
        }

        /// <inheritdoc />
        public async Task<bool> StopHistoryObservationOfOutgoingTransactionsAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            try
            {
                await _runner.RunWithRetriesAsync(() => _api.StopHistoryObservationOfOutgoingTransactionsAsync(address));

                return true;
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> StopHistoryObservationOfIncomingTransactionsAsync(string address)
        {
            ValidateAddressIsNotEmpty(address);

            try
            {
                await _runner.RunWithRetriesAsync(() => _api.StopHistoryObservationOfIncomingTransactionsAsync(address));

                return true;
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
            {
                return false;
            }
        }

        #endregion


        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #endregion


        #region Private

        private async Task<PaginationResult<BlockchainAsset>> GetAssetsAsync(int take, string continuation)
        {
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetAssetsAsync(take, continuation));

            ValidateContractValueIsNotNull(apiResponse);
            ValidateContractItemsIsNotNull(apiResponse.Items);

            return PaginationResult.From(
                apiResponse.Continuation,
                apiResponse.Items.Select(a => new BlockchainAsset(a)).ToArray());
        }

        private async Task<PaginationResult<WalletBalance>> GetWalletBalancesAsync(int take, string continuation, Func<string, int> assetAccuracyProvider)
        {
            ValidateTakeRange(take);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetWalletBalancesAsync(take, continuation));

            ValidateContractValueIsNotNull(apiResponse);
            ValidateContractItemsIsNotNull(apiResponse.Items);

            return PaginationResult.From(
                apiResponse.Continuation,
                apiResponse.Items.Select(b => new WalletBalance(b, assetAccuracyProvider(b.AssetId))).ToArray());
        }

        #endregion


        #region Validation

        private static void ValidateContractValueIsNotNull(object apiResponse)
        {
            if (apiResponse == null)
            {
                throw new ResultValidationException("Contract vaule is required");
            }
        }

        private static void ValidateAssetIdIsNotEmpty(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
            {
                throw new ArgumentException("Asset ID is required", nameof(assetId));
            }
        }

        private static void ValidateAddressIsNotEmpty(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address is required", nameof(address));
            }
        }

        private static void ValidateTakeRange(int take)
        {
            if (take <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take,
                    "Amount of items to take should be positive number");
            }
        }

        private static void ValidateAssetAccuracyProviderIsNotNull(Func<string, int> assetAccuracyProvider)
        {
            if (assetAccuracyProvider == null)
            {
                throw new ArgumentNullException(nameof(assetAccuracyProvider));
            }
        }

        private static void ValidateAmountRange(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount should be positive number");
            }
        }

        private static void ValidateAssetIsNotNull(BlockchainAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("Asset is required", nameof(asset));
            }
        }

        private static void ValidateToAddressIsNotEmpty(string toAddress)
        {
            if (string.IsNullOrWhiteSpace(toAddress))
            {
                throw new ArgumentNullException("Destination address is required", nameof(toAddress));
            }
        }

        private static void ValidateFromAddresIsNotEmpty(string fromAddress)
        {
            if (string.IsNullOrWhiteSpace(fromAddress))
            {
                throw new ArgumentException("Source address is required", nameof(fromAddress));
            }
        }

        private static void ValidateSendTransactionHashIsNotEmpty(string sendTransactionHash)
        {
            if (string.IsNullOrWhiteSpace(sendTransactionHash))
            {
                throw new ArgumentException("Send transaction hash is required", nameof(sendTransactionHash));
            }
        }

        private static void ValidateOperationIdIsNotEmpty(Guid operationId)
        {
            if (operationId == Guid.Empty)
            {
                throw new ArgumentException("Operation ID is required", nameof(operationId));
            }
        }

        private static void ValidateFeeFactorRange(decimal feeFactor)
        {
            if (feeFactor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(feeFactor), feeFactor, "Fee factor should be positive number");
            }
        }

        private static void ValidateSignedTransactionIsNotEmpty(string signedTransaction)
        {
            if (string.IsNullOrWhiteSpace(signedTransaction))
            {
                throw new ArgumentException("Signed transaction is required", nameof(signedTransaction));
            }
        }

        private static void ValidateOperationIdsAreNotEmpty(IReadOnlyList<Guid> operationIds)
        {
            if (operationIds == null)
            {
                throw new ArgumentNullException(nameof(operationIds));
            }
        }

        private static void ValidateInputsNotNull(IEnumerable<BuildingTransactionInput> inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }
        }

        private static void ValidateOutputsNotNull(IEnumerable<BuildingTransactionOutput> outputs)
        {
            if (outputs == null)
            {
                throw new ArgumentNullException(nameof(outputs));
            }
        }

        private void ValidateContractItemsIsNotNull<TItem>(IReadOnlyList<TItem> items)
        {
            if (items == null)
            {
                throw new ResultValidationException("Items are required");
            }
        }

        #endregion
    }
}
