using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Common.Log;
using Lykke.SettingsReader;

namespace Lykke.Service.BlockchainApi.Sdk.Domain
{
    [ValueTypeMergingStrategyAttribute(ValueTypeMergingStrategy.UpdateAlways)]
    public class StateEntity<T> : AzureTableEntity
    {
        public static string Partition() => "State";
        public static string Row() => "";

        public StateEntity() { }
        public StateEntity(T state) => (PartitionKey, RowKey, State) = (Partition(), Row(), state);

        [JsonValueSerializer]
        public T State { get; set; }
    }

    public class StateRepository<T>
    {
        readonly INoSQLTableStorage<StateEntity<T>> _tableStorage;

        public StateRepository(IReloadingManager<string> connectionStringManager, ILogFactory logFactory) =>
            _tableStorage = AzureTableStorage<StateEntity<T>>.Create(connectionStringManager, "State", logFactory);

        public async Task UpsertAsync(T state) =>
            await _tableStorage.InsertOrMergeAsync(new StateEntity<T>(state));

        public async Task<StateEntity<T>> GetAsync() =>
            await _tableStorage.GetDataAsync(StateEntity<T>.Partition(), StateEntity<T>.Row());
    }
}