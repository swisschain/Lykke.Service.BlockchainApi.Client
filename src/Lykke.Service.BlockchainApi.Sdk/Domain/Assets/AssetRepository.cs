using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.SettingsReader;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.Assets
{
    public class AssetRepository
    {
        readonly INoSQLTableStorage<AssetEntity> _tableStorage;
        readonly ConcurrentDictionary<string, AssetEntity> _cache = new ConcurrentDictionary<string, AssetEntity>();

        public AssetRepository(IReloadingManager<string> connectionStringManager, ILogFactory logFactory)
        {
            _tableStorage = AzureTableStorage<AssetEntity>.Create(connectionStringManager, "Assets", logFactory);
        }

        public async Task UpsertAsync(string assetId, string address, string name, int accuracy)
        {
            var asset = new AssetEntity(assetId, address, name, accuracy);
            await _tableStorage.InsertOrReplaceAsync(asset);
            _cache.AddOrUpdate(assetId, asset, (id, _) => asset);
        }

        public async Task<AssetEntity> GetAsync(string assetId)
        {
            if (!_cache.TryGetValue(assetId, out var asset))
            {
                asset = await _tableStorage.GetDataAsync(AssetEntity.Partition(assetId), AssetEntity.Row());
                _cache.TryAdd(assetId, asset);
            }

            return asset;
        }

        public async Task<(IEnumerable<AssetEntity> items, string continuation)> GetAsync(int take, string continuation) =>
            await _tableStorage.GetDataWithContinuationTokenAsync(take, continuation);
    }
}