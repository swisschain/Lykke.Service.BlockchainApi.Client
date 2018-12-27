using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain
{
    public class OperationAction : IOperationAction
    {
        public OperationAction() { }
        public OperationAction(string actionId, string from, string fromContext, string to, decimal amount) =>
            (ActionId, From, FromContext, To, Amount) = (actionId, from, fromContext, to, amount);

        public string  ActionId    { get; set; }
        public string  From        { get; set; }
        public string  FromContext { get; set; }
        public string  To          { get; set; }
        public decimal Amount      { get; set; }

        public bool IsFake(char separator) => From.Split(separator)[0] == To.Split(separator)[0];
        public bool IsReal(char separator) => !IsFake(separator);
    }

    [ValueTypeMergingStrategyAttribute(ValueTypeMergingStrategy.UpdateAlways)]
    public class OperationEntity : AzureTableEntity
    {
        public static string Partition(Guid id) => id.ToString();
        public static string Row() => "";

        public OperationEntity() {}
        public OperationEntity(Guid operationId, string assetId, OperationAction[] actions, bool includeFee, decimal fee = 0, long expiration = 0)
        {
            PartitionKey = Partition(operationId);
            RowKey = Row();
            BuildTime = DateTime.Now;
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

        public bool IsRunning() =>
            SendTime.HasValue || CompletionTime.HasValue || FailTime.HasValue;

        public DateTime GetTimestamp() =>
            FailTime ?? CompletionTime ?? SendTime ?? BuildTime;

        public BroadcastedTransactionState GetState() =>
            FailTime.HasValue ? BroadcastedTransactionState.Failed : CompletionTime.HasValue ? BroadcastedTransactionState.Completed : BroadcastedTransactionState.InProgress;
    }

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

    public class OperationRepository
    {
        readonly INoSQLTableStorage<OperationEntity> _operationStorage;
        readonly INoSQLTableStorage<OperationIndexEntity> _operationIndexStorage;

        public OperationRepository(IReloadingManager<string> connectionStringManager, ILogFactory logFactory)
        {
            _operationStorage = AzureTableStorage<OperationEntity>.Create(connectionStringManager, "Operations", logFactory);
            _operationIndexStorage = AzureTableStorage<OperationIndexEntity>.Create(connectionStringManager, "OperationIndex", logFactory);
        }

        public async Task UpsertAsync(Guid operationId, string assetId, OperationAction[] actions, bool includeFee, decimal fee = 0, long expiration = 0) =>
            await _operationStorage.InsertOrMergeAsync(new OperationEntity(operationId, assetId, actions, includeFee, fee, expiration));

        public async Task<OperationEntity> UpdateAsync(Guid operationId, DateTime? sendTime = null, DateTime? completionTime = null,
            DateTime? blockTime = null, DateTime? failTime = null, DateTime? deleteTime = null, string transactionHash = null, long? blockNumber = null,
            string error = null, BlockchainErrorCode? errorCode = null, string broadcastResult = null)
        {
            if (!string.IsNullOrEmpty(transactionHash))
            {
                await _operationIndexStorage.InsertOrMergeAsync(new OperationIndexEntity(transactionHash, operationId));
            }

            return await _operationStorage.MergeAsync(
                OperationEntity.Partition(operationId),
                OperationEntity.Row(),
                op =>
                {
                    op.SendTime = sendTime ?? op.SendTime;
                    op.CompletionTime = completionTime ?? op.CompletionTime;
                    op.BlockTime = blockTime ?? op.BlockTime;
                    op.FailTime = failTime ?? op.FailTime;
                    op.DeleteTime = deleteTime ?? op.DeleteTime;
                    op.TransactionHash = transactionHash ?? op.TransactionHash;
                    op.BlockNumber = blockNumber ?? op.BlockNumber;
                    op.Error = error ?? op.Error;
                    op.ErrorCode = errorCode ?? op.ErrorCode;
                    op.BroadcastResult = broadcastResult ?? op.BroadcastResult;
                    return op;
                }
            );
        }

        public async Task<OperationEntity> GetAsync(Guid operationId) => 
            await _operationStorage.GetDataAsync(OperationEntity.Partition(operationId), OperationEntity.Row());

        public async Task<OperationIndexEntity> GetOperationIndexAsync(string transactionHash) =>
            await _operationIndexStorage.GetDataAsync(OperationIndexEntity.Partition(transactionHash), OperationIndexEntity.Row());
    }
}