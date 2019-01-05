using System;
using System.Linq;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.Operations
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class OperationEntity : AzureTableEntity
    {
        public static string Partition(Guid id) => id.ToString();
        public static string Row() => "";

        public OperationEntity() {}
        public OperationEntity(Guid operationId, string assetId, OperationAction[] actions, bool includeFee, decimal fee = 0, long expiration = 0)
        {
            PartitionKey = Partition(operationId);
            RowKey = Row();
            BuildTime = DateTime.UtcNow;
            AssetId = assetId;
            Actions = actions;
            Amount = actions.Sum(e => e.Amount);
            IncludeFee = includeFee;
            Fee = fee;
            Expiration = expiration;
        }

        [IgnoreProperty] 
        public Guid                 OperationId     { get => Guid.Parse(PartitionKey); }
        [JsonValueSerializer]
        public OperationAction[]    Actions         { get; set; }
        public string               AssetId         { get; set; }
        public decimal              Amount          { get; set; }
        public bool                 IncludeFee      { get; set; }
        public decimal              Fee             { get; set; }
        public DateTime             BuildTime       { get; set; }
        public DateTime?            SendTime        { get; set; }
        public DateTime?            CompletionTime  { get; set; }
        public DateTime?            BlockTime       { get; set; }
        public DateTime?            FailTime        { get; set; }
        public DateTime?            DeleteTime      { get; set; }
        public string               TransactionHash { get; set; }
        public long?                BlockNumber     { get; set; }
        public BlockchainErrorCode? ErrorCode       { get; set; }
        public string               Error           { get; set; }
        public string               BroadcastResult { get; set; }
        public long                 Expiration      { get; set; }

        public DateTime GetTimestamp() =>
            FailTime ?? CompletionTime ?? SendTime ?? BuildTime;

        public BroadcastedTransactionState GetState() =>
            FailTime.HasValue ? BroadcastedTransactionState.Failed : CompletionTime.HasValue ? BroadcastedTransactionState.Completed : BroadcastedTransactionState.InProgress;
    }
}