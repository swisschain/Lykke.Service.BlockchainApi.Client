using System;
using System.Threading.Tasks;

namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Handles blockchain state.
    /// </summary>
    /// <typeparam name="TState">Type of object to keep state between calls of <see cref="IBlockchainJob.TraceDepositsAsync()"/>.</typeparam>
    public interface IBlockchainJob<TState>
    {
        /// <summary>
        /// Called periodically. Should return new blockchain actions (if any) since last call.
        /// </summary>
        /// <param name="state">State from last call (processed height, last processed transaction hash, etc.)</param>
        /// <param name="getAsset">Function to get asset by its identifier</param>
        /// <returns>New actions (if any) since last call and current state, if necessary.</returns>
        Task<(BlockchainAction[] actions, TState state)> TraceDepositsAsync(TState state, Func<string, Task<IAsset>> getAsset);
    }
}