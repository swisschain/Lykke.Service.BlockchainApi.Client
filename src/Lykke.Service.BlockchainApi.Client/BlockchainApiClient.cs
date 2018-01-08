using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Client.Results;
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

        public BlockchainApiClient(string hostUrl, int retriesCount = 5)
        {
            HostUrl = hostUrl ?? throw new ArgumentNullException(nameof(hostUrl));

            _httpClient = new HttpClient
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
        public async Task<IEnumerable<BlockchainAsset>> GetAssetsAsync()
        {
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetAssetsAsync());

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(a => new BlockchainAsset(a));
        }
        
        /// <inheritdoc />
        public async Task<BlockchainAsset> GetAssetAsync(string assetId)
        {
            ValidateAssetIdIsNotEmpty(assetId);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetAssetAsync(assetId));

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
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
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
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WalletBalance>> GetWalletBalancesAsync(int take, int skip, Func<string, int> assetAccuracyProvider)
        {
            ValidateTakeSkipRange(take, skip);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetWalletBalancesAsync(take, skip));

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(b => new WalletBalance(b, assetAccuracyProvider(b.AssetId)));
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

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.BuildTransactionAsync(new BuildTransactionRequest
            {
                OperationId = operationId,
                FromAddress = fromAddress,
                ToAddress = toAddress,
                AssetId = asset.AssetId,
                Amount = Conversions.CoinsToContract(amount, asset.Accuracy),
                IncludeFee = includeFee
            }));

            return new TransactionBuildingResult(apiResponse);
        }

        /// <inheritdoc />
        public async Task<TransactionBuildingResult> RebuildTransactionAsync(
            Guid operationId, 
            string fromAddress, 
            string toAddress, 
            BlockchainAsset asset, 
            decimal amount,
            bool includeFee, 
            decimal feeFactor)
        {
            ValidateOperationIdIsNotEmpty(operationId);
            ValidateFromAddresIsNotEmpty(fromAddress);
            ValidateToAddressIsNotEmpty(toAddress);
            ValidateAssetIsNotNull(asset);
            ValidateAmountRange(amount);
            ValidateFeeFactorRange(feeFactor);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.RebuildTransactionAsync(new RebuildTransactionRequest
            {
                OperationId = operationId,
                FromAddress = fromAddress,
                ToAddress = toAddress,
                AssetId = asset.AssetId,
                Amount = Conversions.CoinsToContract(amount, asset.Accuracy),
                IncludeFee = includeFee,
                FeeFactor = feeFactor
            }));

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
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<InProgressTransaction>> GetInProgressTransactionsAsync(int take, int skip, Func<string, int> assetAccuracyProvider)
        {
            ValidateTakeSkipRange(take, skip);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetInProgressTransactionsAsync(take, skip));

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(t => new InProgressTransaction(t, assetAccuracyProvider(t.AssetId)));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CompletedTransaction>> GetCompletedTransactionsAsync(int take, int skip, Func<string, int> assetAccuracyProvider)
        {
            ValidateTakeSkipRange(take, skip);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetCompletedTransactionsAsync(take, skip));

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(t => new CompletedTransaction(t, assetAccuracyProvider(t.AssetId)));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<FailedTransaction>> GetFailedTransactionsAsync(int take, int skip, Func<string, int> assetAccuracyProvider)
        {
            ValidateTakeSkipRange(take, skip);
            ValidateAssetAccuracyProviderIsNotNull(assetAccuracyProvider);

            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetFailedTransactionsAsync(take, skip));

            ValidateContractValueIsNotNull(apiResponse);

            return apiResponse.Select(t => new FailedTransaction(t, assetAccuracyProvider(t.AssetId)));
        }

        /// <inheritdoc />
        public Task StopTransactionsObservationAsync(IReadOnlyList<Guid> operationIds)
        {
            ValidateOperationIdsAreNotEmpty(operationIds);

            if (!operationIds.Any())
            {
                return Task.CompletedTask;
            }

            return _runner.RunWithRetriesAsync(() => _api.StopTransactionsObservationAsync(operationIds));
        }

        #endregion


        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient?.Dispose();
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

        private static void ValidateTakeSkipRange(int take, int skip)
        {
            if (take <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "Amount of items to take should be positive number");
            }
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), skip, "Amount of items to skip should be not negative number");
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

        #endregion
    }
}
