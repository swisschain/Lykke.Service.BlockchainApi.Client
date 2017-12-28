using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Results;
using Lykke.Service.BlockchainApi.Contract.Requests;
using Lykke.Service.BlockchainApi.Contract.Responses;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class BlockchainEventsHandlerClient : IBlockchainEventsHandlerClient
    {
        private readonly HttpClient _httpClient;
        private readonly IBlockchainEventsHandlerApi _api;
        private readonly ApiRunner _runner;

        public BlockchainEventsHandlerClient(ILog log, string hostUrl)
        {
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

            _api = RestService.For<IBlockchainEventsHandlerApi>(_httpClient);

            _runner = new ApiRunner(log, defaultRetriesCount: 5);
        }

        /// <inheritdoc />
        public Task<IsAliveResponse> GetIsAliveAsync()
        {
            return _runner.RunAsync(() => _api.GetIsAliveAsync());
        }

        public Task<AssetsListResponse> GetAssetsAsync()
        {
            return _runner.RunWithRetriesAsync(() => _api.GetAssetsAsync());
        }

        public Task<AssetResponse> GetAssetAsync(string assetId)
        {
            return _runner.RunWithRetriesAsync(() => _api.GetAssetAsync(assetId));
        }

        public async Task<bool> IsAddressValidAsync(string address)
        {
            return (await _runner.RunWithRetriesAsync(() => _api.IsAddressValidAsync(address))).IsValid;
        }

        public Task<WalletCreationResponse> CreateWalletAsync()
        {
            return _runner.RunAsync(() => _api.CreateWalletAsync());
        }

        public async Task CashoutFromWalletAsync(string address, string toAddress, string assetId, decimal amount, IReadOnlyList<string> signers)
        {
            var asset = await GetAssetAsync(assetId);

            var contractAmount = Math.Round(amount * asset.Accuracy).ToString(CultureInfo.InvariantCulture);

            await _runner.RunAsync(() => _api.CashoutFromWalletAsync(address, new CashoutFromWalletRequest
            {
                To = toAddress,
                AssetId = assetId,
                Amount = contractAmount,
                Signers = signers
            }));
        }
        
        public async Task<PendingEventsResponse<PendingCashinEvent>> GetPendingCashinEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashinEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return new PendingEventsResponse<PendingCashinEvent>
            {
                Events = apiResponse.Events
                    .Select(e => new PendingCashinEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                    .ToArray()
            };
        }

        public async Task<PendingEventsResponse<PendingCashoutStartedEvent>> GetPendingCashoutStartedEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashoutStartedEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return new PendingEventsResponse<PendingCashoutStartedEvent>
            {
                Events = apiResponse.Events
                    .Select(e => new PendingCashoutStartedEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                    .ToArray()
            };
        }

        public async Task<PendingEventsResponse<PendingCashoutCompletedEvent>> GetPendingCashoutCompletedEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashoutCompletedEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return new PendingEventsResponse<PendingCashoutCompletedEvent>
            {
                Events = apiResponse.Events
                    .Select(e => new PendingCashoutCompletedEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                    .ToArray()
            };
        }

        public async Task<PendingEventsResponse<PendingCashoutFailedEvent>> GetPendingCashoutFailedEventsAsync(int maxEventsNumber)
        {
            var assetsTask = GetAssetsAsync();
            var apiResponse = await _runner.RunWithRetriesAsync(() => _api.GetPendingCashoutFailedEventsAsync(maxEventsNumber));
            var assets = (await assetsTask)
                .Assets
                .ToDictionary(a => a.AssetId, a => a);

            return new PendingEventsResponse<PendingCashoutFailedEvent>
            {
                Events = apiResponse.Events
                    .Select(e => new PendingCashoutFailedEvent(e, GetAssetAccuracy(assets, e.AssetId)))
                    .ToArray()
            };
        }

        public Task RemovePendingCashinEventsAsync(IReadOnlyList<string> operationIds)
        {
            return _runner.RunWithRetriesAsync(() => _api.RemovePendingCashinEventsAsync(new RemovePendingEventsRequest
            {
                OperationIds = operationIds
            }));
        }

        public Task RemovePendingCashoutStartedEventsAsync(IReadOnlyList<string> operationIds)
        {
            return _runner.RunWithRetriesAsync(() => _api.RemovePendingCashoutStartedEventsAsync(new RemovePendingEventsRequest
            {
                OperationIds = operationIds
            }));
        }

        public Task RemovePendingCashoutCompletedEventsAsync(IReadOnlyList<string> operationIds)
        {
            return _runner.RunWithRetriesAsync(() => _api.RemovePendingCashoutCompletedEventsAsync(new RemovePendingEventsRequest
            {
                OperationIds = operationIds
            }));
        }

        public Task RemovePendingCashoutFailedEventsAsync(IReadOnlyList<string> operationIds)
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

        private int GetAssetAccuracy(Dictionary<string, AssetResponse> assets, string assetId)
        {
            if (!assets.TryGetValue(assetId, out var asset))
            {
                throw new InvalidOperationException($"Asset [{assetId}] not found in the blockchain [{_httpClient.BaseAddress}]");
            }
            return asset.Accuracy;
        }
    }
}