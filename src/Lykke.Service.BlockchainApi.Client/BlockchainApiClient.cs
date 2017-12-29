using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Results;
using Lykke.Service.BlockchainApi.Client.Results.PendingEvents;
using Lykke.Service.BlockchainApi.Contract.Requests;
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

        /// <inheritdoc />
        public Task<IsAliveResponse> GetIsAliveAsync()
        {
            return _runner.RunAsync(() => _api.GetIsAliveAsync());
        }

        public async Task<BlockchainAssetsList> GetAssetsAsync()
        {
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetAssetsAsync());

            return new BlockchainAssetsList(apiResponse);
        }

        public async Task<BlockchainAsset> GetAssetAsync(string assetId)
        {
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetAssetAsync(assetId));

            return new BlockchainAsset(apiResponse);
        }

        public async Task<bool> IsAddressValidAsync(string address)
        {
            return (await _runner.RunWithRetriesAsync(() => _api.IsAddressValidAsync(address))).IsValid;
        }

        public async Task<WalletCreationResult> CreateWalletAsync()
        {
            var apiResponse = await _runner.RunAsync(() => _api.CreateWalletAsync());

            return new WalletCreationResult(apiResponse);
        }

        public async Task CashoutFromWalletAsync(string address, string toAddress, string assetId, decimal amount, IReadOnlyList<string> signers)
        {
            var asset = await GetAssetAsync(assetId);

            var assetPow = (decimal)Math.Pow(10, asset.Accuracy);
            var contractAmount = Math.Round(amount * assetPow).ToString(CultureInfo.InvariantCulture);

            await _runner.RunAsync(() => _api.CashoutFromWalletAsync(address, new CashoutFromWalletRequest
            {
                To = toAddress,
                AssetId = assetId,
                Amount = contractAmount,
                Signers = signers
            }));
        }
        
        public async Task<PendingEventsList<PendingCashinEvent>> GetPendingCashinEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashinEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return PendingEventsList.From(apiResponse
                .Events
                .Select(e => new PendingCashinEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                .ToArray());
        }

        public async Task<PendingEventsList<PendingCashoutStartedEvent>> GetPendingCashoutStartedEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashoutStartedEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return PendingEventsList.From(apiResponse
                .Events
                .Select(e => new PendingCashoutStartedEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                .ToArray());
        }

        public async Task<PendingEventsList<PendingCashoutCompletedEvent>> GetPendingCashoutCompletedEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashoutCompletedEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return PendingEventsList.From(apiResponse
                .Events
                .Select(e => new PendingCashoutCompletedEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                .ToArray());
        }

        public async Task<PendingEventsList<PendingCashoutFailedEvent>> GetPendingCashoutFailedEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashoutFailedEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return PendingEventsList.From(apiResponse
                .Events
                .Select(e => new PendingCashoutFailedEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                .ToArray());
        }

        public Task RemovePendingCashinEventsAsync(IReadOnlyList<Guid> operationIds)
        {
            return _runner.RunWithRetriesAsync(() => _api.RemovePendingCashinEventsAsync(new RemovePendingEventsRequest
            {
                OperationIds = operationIds
            }));
        }

        public Task RemovePendingCashoutStartedEventsAsync(IReadOnlyList<Guid> operationIds)
        {
            return _runner.RunWithRetriesAsync(() => _api.RemovePendingCashoutStartedEventsAsync(new RemovePendingEventsRequest
            {
                OperationIds = operationIds
            }));
        }

        public Task RemovePendingCashoutCompletedEventsAsync(IReadOnlyList<Guid> operationIds)
        {
            return _runner.RunWithRetriesAsync(() => _api.RemovePendingCashoutCompletedEventsAsync(new RemovePendingEventsRequest
            {
                OperationIds = operationIds
            }));
        }

        public Task RemovePendingCashoutFailedEventsAsync(IReadOnlyList<Guid> operationIds)
        {
            return _runner.RunWithRetriesAsync(() => _api.RemovePendingCashoutFailedEventsAsync(new RemovePendingEventsRequest
            {
                OperationIds = operationIds
            }));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private int GetAssetAccuracy(Dictionary<string, BlockchainAsset> assets, string assetId)
        {
            if (!assets.TryGetValue(assetId, out var asset))
            {
                throw new InvalidOperationException($"Asset [{assetId}] not found in the blockchain [{_httpClient.BaseAddress}]");
            }
            return asset.Accuracy;
        }
    }
}
