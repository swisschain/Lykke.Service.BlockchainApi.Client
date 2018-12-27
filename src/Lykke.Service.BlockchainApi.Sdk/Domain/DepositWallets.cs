using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Common.Log;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BlockchainApi.Sdk.Domain
{
    public class DepositWalletEntity : TableEntity
    {
        public static string Partition(string address) => address;
        public static string Row() => "";

        public DepositWalletEntity() {}
        public DepositWalletEntity(string address) => (PartitionKey, RowKey) = (Partition(address), Row());

        [IgnoreProperty]
        public string Address { get => PartitionKey; }
    }

    [ValueTypeMergingStrategyAttribute(ValueTypeMergingStrategy.UpdateAlways)]
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

    [ValueTypeMergingStrategyAttribute(ValueTypeMergingStrategy.UpdateAlways)]
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

    public class DepositWalletRepository
    {
        readonly INoSQLTableStorage<DepositActionEntity> _actionStorage;
        readonly INoSQLTableStorage<DepositWalletEntity> _walletStorage;
        readonly INoSQLTableStorage<DepositWalletBalanceEntity> _walletBalanceStorage;

        public DepositWalletRepository(IReloadingManager<string> connectionStringManager, ILogFactory logFactory)
        {
            _actionStorage = AzureTableStorage<DepositActionEntity>.Create(connectionStringManager, "DepositActions", logFactory);
            _walletStorage = AzureTableStorage<DepositWalletEntity>.Create(connectionStringManager, "DepositWallets", logFactory);
            _walletBalanceStorage = AzureTableStorage<DepositWalletBalanceEntity>.Create(connectionStringManager, "DepositWalletBalances", logFactory);
        }

        public async Task<bool> TryObserveAsync(string address) =>
            await _walletStorage.CreateIfNotExistsAsync(new DepositWalletEntity(address));

        public async Task<bool> IsObservedAsync(string address) =>
            await _walletStorage.RecordExistsAsync(new DepositWalletEntity(address));

        public async Task<bool> TryDeleteObservationAsync(string address)
        {
            // delete wallet if exists
            var existed = await _walletStorage.DeleteIfExistAsync(DepositWalletEntity.Partition(address), DepositWalletEntity.Row());

            // if not deleted earlier then delete balances
            if (existed)
            {
                string continuation = null;

                do
                {
                    var query = new TableQuery<DepositWalletBalanceEntity>().Where($"PartitionKey eq '{DepositWalletBalanceEntity.Partition(address)}'");
                    var chunk = await _walletBalanceStorage.GetDataWithContinuationTokenAsync(query, 100, continuation);
                    var batch = new TableBatchOperation();

                    continuation = chunk.ContinuationToken;

                    foreach (var balance in chunk.Entities)
                        batch.Delete(balance);

                    if (batch.Any())
                        await _walletBalanceStorage.DoBatchAsync(batch);

                } while (!string.IsNullOrEmpty(continuation));           
            }

            return existed;
        }

        public async Task UpsertActionAsync(string address, string assetId, long blockNumber, string transactionHash, string actionId, decimal amount, Guid? operationId = null) =>
            await _actionStorage.InsertOrMergeAsync(new DepositActionEntity(address, assetId, blockNumber, transactionHash, actionId, amount, operationId));

        public async Task RefreshBalanceAsync(IEnumerable<(string address, string assetId)> wallets)
        {
            if (wallets.Any())
            {
                using (var semaphore = new SemaphoreSlim(10))
                {
                    var tasks = wallets.Select(async x =>
                    {
                        try
                        {
                            await semaphore.WaitAsync();
                            await RefreshBalanceAsync(x.address, x.assetId);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    await Task.WhenAll(tasks);
                }
            }
        }

        public async Task RefreshBalanceAsync(string address, string assetId)
        {
            var actions = await _actionStorage.GetDataAsync(DepositActionEntity.Partition(address, assetId));
            var balance = actions.Aggregate(
                new DepositWalletBalanceEntity(address, assetId),
                (b, j) =>
                {
                    b.Amount += j.Amount;
                    b.BlockNumber = Math.Max(b.BlockNumber, j.BlockNumber);
                    return b;
                }
            );

            if (balance.Amount == 0)
                await _walletBalanceStorage.DeleteIfExistAsync(DepositWalletBalanceEntity.Partition(address), DepositWalletBalanceEntity.Row(assetId));
            else
                await _walletBalanceStorage.InsertOrMergeAsync(balance);
        }

        public async Task EnrollIfObservedAsync(IEnumerable<BlockchainAction> actions, Guid? operationId = null)
        {
            var wallets = new HashSet<(string, string)>();

            foreach (var action in actions)
            {
                var wallet = (action.Address, action.AssetId);

                if (wallets.Contains(wallet) || 
                    await IsObservedAsync(action.Address))
                {
                    // cache observed address
                    wallets.Add(wallet);

                    await UpsertActionAsync(action.Address, action.AssetId, 
                        action.BlockNumber, action.TransactionHash, action.ActionId, action.Amount, operationId);
                }
            }

            await RefreshBalanceAsync(wallets);
        }

        public async Task<(IEnumerable<DepositWalletBalanceEntity> items, string continuation)> GetBalanceAsync(int take, string continuation) =>
            await _walletBalanceStorage.GetDataWithContinuationTokenAsync(take, continuation);

        public async Task<DepositWalletBalanceEntity> GetBalanceAsync(string address, string assetId) =>
            await _walletBalanceStorage.GetDataAsync(DepositWalletBalanceEntity.Partition(address), DepositWalletBalanceEntity.Row(assetId));

        public async Task<(IEnumerable<DepositWalletEntity> items, string continuation)> GetWalletsAsync(int take, string continuation) =>
            await _walletStorage.GetDataWithContinuationTokenAsync(take, continuation);
    }
}