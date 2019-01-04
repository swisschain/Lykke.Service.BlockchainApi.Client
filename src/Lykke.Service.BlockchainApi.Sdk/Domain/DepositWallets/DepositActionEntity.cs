using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.DepositWallets
{
    public class DepositActionEntity : AzureTableEntity
    {
        public static string Partition(string address, string assetId) => $"{address}_{assetId}";
        public static string Row(string hash, string actionId) => $"{hash}_{actionId}";

        public DepositActionEntity() { }
        public DepositActionEntity(string address, string assetId, long blockNumber, string transactionHash, string actionId, decimal amount, Guid? operationId = null)
        {
            PartitionKey = Partition(address, assetId);
            RowKey = Row(transactionHash, actionId);
            Address = address;
            AssetId = assetId;
            BlockNumber = blockNumber;
            TransactionHash = transactionHash;
            ActionId = actionId;
            Amount = amount;
            OperationId = operationId;
        }

        public string  Address         { get; set; }
        public string  AssetId         { get; set; }
        public long    BlockNumber     { get; set; }
        public string  TransactionHash { get; set; }
        public string  ActionId        { get; set; }
        public decimal Amount          { get; set; }
        public Guid?   OperationId     { get; set; }
    }
}
