using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Common;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Blockchain constants
    /// </summary>
    [PublicAPI]
    public class BlockchainConstants
    {
        [CanBeNull]
        public PublicAddressExtensionConstants PublicAddressExtension { get; }

        public BlockchainConstants(ConstantsResponse contract)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (contract == null)
            {
                throw new ResultValidationException("Transaction not found");
            }

            PublicAddressExtension = contract.PublicAddressExtension != null
                ? new PublicAddressExtensionConstants(contract.PublicAddressExtension)
                : null;
        }
    }
}
