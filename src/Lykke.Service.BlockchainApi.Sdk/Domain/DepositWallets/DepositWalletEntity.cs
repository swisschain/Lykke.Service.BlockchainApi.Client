using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.DepositWallets
{
    public class DepositWalletEntity : TableEntity
    {
        public static string Partition(string address) => address;
        public static string Row() => "";

        public DepositWalletEntity() { }
        public DepositWalletEntity(string address) => (PartitionKey, RowKey) = (Partition(address), Row());

        [IgnoreProperty]
        public string Address { get => PartitionKey; }
    }
}
