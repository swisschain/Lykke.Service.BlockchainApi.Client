using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.SettingsReader;

namespace Lykke.Service.BlockchainApi.Sdk.Domain.State
{
    public class StateRepository<TState>
    {
        readonly INoSQLTableStorage<StateEntity<TState>> _tableStorage;

        public StateRepository(IReloadingManager<string> connectionStringManager, ILogFactory logFactory) =>
            _tableStorage = AzureTableStorage<StateEntity<TState>>.Create(connectionStringManager, "State", logFactory);

        public async Task UpsertAsync(TState state) =>
            await _tableStorage.InsertOrReplaceAsync(new StateEntity<TState>(state));

        public async Task<StateEntity<TState>> GetAsync() =>
            await _tableStorage.GetDataAsync(StateEntity<TState>.Partition(), StateEntity<TState>.Row());
    }
}