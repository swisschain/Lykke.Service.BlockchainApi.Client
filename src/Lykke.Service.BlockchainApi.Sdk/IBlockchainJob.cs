using System;
using System.Threading.Tasks;

namespace Lykke.Service.BlockchainApi.Sdk
{
    public interface IBlockchainJob<T>
    {
        Task<(BlockchainAction[] actions, T state)> TraceDepositsAsync(T state, Func<string, Task<IAsset>> getAsset);
    }
}