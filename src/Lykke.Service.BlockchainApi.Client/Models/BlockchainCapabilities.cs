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

        /// <summary>
        /// If blockchain requires additional field to represent
        /// public address to use it as a deposit destination, 
        /// then this flag should be true.
        /// <see cref="BlockchainConstants.PublicAddressExtension"/> should be non empty,
        /// if this flag is true.
        /// For example: Address Tag in the Ripple.
        /// </summary>
        public bool IsPublicAddressExtensionRequired { get; set; }

        /// <summary>
        /// If blockchain requires broadcasting of the “receive”
        /// transaction in order to accomplish funds transferring
        /// to the destination address, then this flag should be
        /// true and <see cref="IBlockchainApiClient.BuildSingleReceiveTransactionAsync"/>
        /// method should be implemented.
        /// </summary>
        public bool IsReceiveTransactionRequired { get; set; }

        /// <summary>
        /// Should be true if
        /// <see cref="IBlockchainApiClient.GetAddressExplorerUrlAsync"/>
        /// is supported.
        /// </summary>
        public bool CanReturnExplorerUrl { get; set; }

        public BlockchainCapabilities(CapabilitiesResponse contract)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (contract == null)
            {
                throw new ResultValidationException("Capabilities not found");
            }

            IsTransactionsRebuildingSupported = contract.IsTransactionsRebuildingSupported ?? false;
            AreManyInputsSupported = contract.AreManyInputsSupported ?? false;
            AreManyOutputsSupported = contract.AreManyOutputsSupported ?? false;
            IsPublicAddressExtensionRequired = contract.IsPublicAddressExtensionRequired ?? false;
            IsReceiveTransactionRequired = contract.IsReceiveTransactionRequired ?? false;
            CanReturnExplorerUrl = contract.CanReturnExplorerUrl ?? false;
        }
    }
}
