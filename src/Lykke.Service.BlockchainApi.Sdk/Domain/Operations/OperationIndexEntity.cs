using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.Operations
{
    public class OperationIndexEntity : TableEntity
    {
        public static string Partition(string transactionHash) => transactionHash;
        public static string Row() => "";

        public OperationIndexEntity() {}
        public OperationIndexEntity(string transactionHash, Guid operationId) => 
            (PartitionKey, RowKey, OperationId) = (Partition(transactionHash), Row(), operationId);

        [IgnoreProperty]
        public string TransactionHash { get => PartitionKey; }
        public Guid   OperationId     { get; set; }
    }
}