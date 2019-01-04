using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.State
{
    public class StateEntity<TState> : AzureTableEntity
    {
        public static string Partition() => "State";
        public static string Row() => "";

        public StateEntity() { }
        public StateEntity(TState state) => (PartitionKey, RowKey, State) = (Partition(), Row(), state);

        [JsonValueSerializer]
        public TState State { get; set; }
    }
}
