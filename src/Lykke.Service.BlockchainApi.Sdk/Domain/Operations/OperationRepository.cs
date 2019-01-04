using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.SettingsReader;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.Operations
{
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
            await _operationStorage.InsertOrReplaceAsync(new OperationEntity(operationId, assetId, actions, includeFee, fee, expiration));

        public async Task<OperationEntity> UpdateAsync(Guid operationId, DateTime? sendTime = null, DateTime? completionTime = null,
            DateTime? blockTime = null, DateTime? failTime = null, DateTime? deleteTime = null, string transactionHash = null, long? blockNumber = null,
            string error = null, BlockchainErrorCode? errorCode = null, string broadcastResult = null)
        {
            if (!string.IsNullOrEmpty(transactionHash))
            {
                await _operationIndexStorage.InsertOrReplaceAsync(new OperationIndexEntity(transactionHash, operationId));
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