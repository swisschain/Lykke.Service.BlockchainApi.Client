using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class BlockchainApiClient : IBlockchainApiClient
    {
        public string HostUrl { get; }

        private readonly HttpClient _httpClient;
        private readonly IBlockchainApi _api;
        private readonly ApiRunner _runner;

        public BlockchainApiClient(ILog log, string hostUrl, int retriesCount = 5)
        {
            HostUrl = hostUrl ?? throw new ArgumentNullException(nameof(hostUrl));

            _httpClient = new HttpClient(new HttpErrorLoggingHandler(log))
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

            _api = RestService.For<IBlockchainApi>(_httpClient);

            _runner = new ApiRunner(retriesCount);
        }


        #region General

        /// <inheritdoc />
        public Task<IsAliveResponse> GetIsAliveAsync()
        {
            return _runner.RunAsync(() => _api.GetIsAliveAsync());
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


        #region Transactions

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> BuildTransactionAsync(Guid operationId, string fromAddress, string toAddress, BlockchainAsset asset, decimal amount, bool includeFee)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateFromAddresIsNotEmpty(fromAddress);
            ValidateToAddressIsNotEmpty(toAddress);
            ValidateAssetIsNotNull(asset);
            ValidateAmountRange(amount);

            BuildTransactionResponse apiResponse;

            try
            {
                apiResponse = await _runner.RunWithRetriesAsync(() => _api.BuildTransactionAsync(
                    new BuildTransactionRequest
                    {
                        OperationId = operationId,
                        FromAddress = fromAddress,
                        ToAddress = toAddress,
                        AssetId = asset.AssetId,
                        Amount = Conversions.CoinsToContract(amount, asset.Accuracy),
                        IncludeFee = includeFee
                    }));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotAcceptable)
            {
                throw new NonAcceptableAmountException($"Transaction amount {amount} is non acceptable", ex);
            }

            return new TransactionBuildingResult(apiResponse);
        }

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> RebuildTransactionAsync(
            Guid operationId, 
            decimal feeFactor)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateFeeFactorRange(feeFactor);

            RebuildTransactionResponse apiResponse;

            try
            {
                apiResponse = await _runner.RunWithRetriesAsync(() => _api.RebuildTransactionAsync(
                    new RebuildTransactionRequest
                    {
                        OperationId = operationId,
                        FeeFactor = feeFactor
                    }));
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.NotAcceptable)
            {
                throw new NonAcceptableAmountException("Transaction amount is non acceptable", ex);
            }

            return new TransactionBuildingResult(apiResponse);
        }

        /// <inheritdoc />
        public async Task<bool> BroadcastTransactionAsync(Guid operationId, string signedTransaction)
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
            }
            catch (ErrorResponseException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<BroadcastedTransaction> TryGetBroadcastedTransactionAsync(Guid operationId, BlockchainAsset asset)
        {
            try
            {
                return await GetBroadcastedTransactionAsync(operationId, asset);
            }
            catch (ErrorResponseException ex) when(ex.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }
        }

        public async Task<BroadcastedTransaction> GetBroadcastedTransactionAsync(Guid operationId, BlockchainAsset asset)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateAssetIsNotNull(asset);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetBroadcastedTransactionAsync(operationId));

            return new BroadcastedTransaction(apiResponse, asset.Accuracy);
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
            ValidateAfterHashIsNotEmpty(afterHash);
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
            ValidateAfterHashIsNotEmpty(afterHash);
            ValidateTakeRange(take);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetHistoryOfIncomingTransactionsAsync(address, afterHash, take));

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(t => new HistoricalTransaction(t, assetAccuracyProvider(t.AssetId)));
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

        private void ValidateAfterHashIsNotEmpty(string afterHash)
        {
            if (string.IsNullOrWhiteSpace(afterHash))
            {
                throw new ArgumentException("'After hash' is required", nameof(afterHash));
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
