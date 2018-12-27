using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain
{
    public class AssetEntity : TableEntity, IAsset
    {
        public static string Partition(string assetId) => assetId;
        public static string Row() => string.Empty;

        public AssetEntity() {}
        public AssetEntity(string assetid, string address, string name, int accuracy) => 
            (PartitionKey, RowKey, Address, Name, Accuracy) = (Partition(assetid), Row(), address, name, accuracy);

        [IgnoreProperty] 
        public string AssetId  { get => PartitionKey; }
        public string Address  { get; set; }
        public string Name     { get; set; }
        public int    Accuracy { get; set; }

        public decimal FromBaseUnit(long amount) =>
            amount / Convert.ToDecimal(Math.Pow(10, Accuracy));

        public long ToBaseUnit(decimal amount) =>
            Convert.ToInt64(Math.Round(amount * Convert.ToDecimal(Math.Pow(10, Accuracy))));

        public AssetResponse ToResponse() => new AssetResponse { AssetId = AssetId, Address = Address, Name = Name, Accuracy = Accuracy };

        public AssetContract ToContract() => ToResponse();
    }

    public class AssetRepository
    {
        readonly INoSQLTableStorage<AssetEntity> _tableStorage;

        public AssetRepository(IReloadingManager<string> connectionStringManager, ILogFactory logFactory) =>
            _tableStorage = AzureTableStorage<AssetEntity>.Create(connectionStringManager, "Assets", logFactory);

        public async Task UpsertAsync(string assetId, string address, string name, int accuracy) => 
            await _tableStorage.InsertOrMergeAsync(new AssetEntity(assetId, address, name, accuracy));

        public async Task<AssetEntity> GetAsync(string assetId) => 
            await _tableStorage.GetDataAsync(AssetEntity.Partition(assetId), AssetEntity.Row());

        public async Task<(IEnumerable<AssetEntity> items, string continuation)> GetAsync(int take, string continuation) =>
            await _tableStorage.GetDataWithContinuationTokenAsync(take, continuation);

        public Func<string, Task<IAsset>> GetCachedAsync()
        {
            var cache = new Dictionary<string, IAsset>();

            return async assetId =>
            {
                if (!cache.TryGetValue(assetId, out var asset))
                    cache.Add(assetId, asset = await GetAsync(assetId));

                return asset;
            };
        }
    }
}