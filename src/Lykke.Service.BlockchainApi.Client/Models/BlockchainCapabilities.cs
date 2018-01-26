using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Common;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Blockchain capabilities
    /// </summary>
    [PublicAPI]
    public class BlockchainCapabilities
    {
        /// <summary>
        /// Should be true, if <see cref="IBlockchainApiClient.RebuildTransactionAsync"/> is supported
        /// </summary>
        public bool IsTransactionsRebuildingSupported { get; }

        /// <summary>
        /// Should be true if 
        /// <see cref="IBlockchainApiClient.BuildTransactionWithManyInputsAsync"/> and
        /// <see cref="IBlockchainApiClient.GetBroadcastedTransactionWithManyInputsAsync"/> 
        /// are supported
        /// </summary>
        public bool AreManyInputsSupported { get; }

        /// <summary>
        /// Should be true if 
        /// <see cref="IBlockchainApiClient.BuildTransactionWithManyOutputsAsync"/> and
        /// <see cref="IBlockchainApiClient.GetBroadcastedTransactionWithManyOutputsAsync"/> 
        /// are supported
        /// </summary>
        public bool AreManyOutputsSupported { get; }

        public BlockchainCapabilities(CapabilitiesResponse contract)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (contract == null)
            {
                throw new ResultValidationException("Capabilities not found");
            }

            IsTransactionsRebuildingSupported = contract.IsTransactionsRebuildingSupported;
            AreManyInputsSupported = contract.AreManyInputsSupported;
            AreManyOutputsSupported = contract.AreManyOutputsSupported;
        }
    }
}
