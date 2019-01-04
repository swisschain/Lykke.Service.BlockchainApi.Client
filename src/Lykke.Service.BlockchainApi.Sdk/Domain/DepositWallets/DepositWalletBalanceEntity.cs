using Lykke.AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.DepositWallets
{
    public class DepositWalletBalanceEntity : AzureTableEntity
    {
        public static string Partition(string address) => address;
        public static string Row(string assetId) => assetId;

        public DepositWalletBalanceEntity() {}
        public DepositWalletBalanceEntity(string address, string assetId) => (PartitionKey, RowKey) = (Partition(address), Row(assetId));

        [IgnoreProperty] 
        public string  Address     { get => PartitionKey; }
        [IgnoreProperty] 
        public string  AssetId     { get => RowKey; }
        public decimal Amount      { get; set; }
        public long    BlockNumber { get; set; }
    }
}
