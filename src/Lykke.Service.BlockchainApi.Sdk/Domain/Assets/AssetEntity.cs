using System;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.Assets
{
    public class AssetEntity : TableEntity, IAsset
    {
        public static string Partition(string assetId) => assetId;
        public static string Row() => string.Empty;

        public AssetEntity() { }
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
}
