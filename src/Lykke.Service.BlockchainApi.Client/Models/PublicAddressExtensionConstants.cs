using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Common;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Blockchain public address extension constants
    /// </summary>
    [PublicAPI]
    public class PublicAddressExtensionConstants
    {
        /// <summary>
        /// Separator character of the main part and extension
        /// part of the wallet public address.
        /// Should be a single character.
        /// Implementation of the Blockchain.SignService and
        /// the Blockchain.API should return main and extension
        /// parts of the public address separated by this
        /// character as atomic address. 
        /// Lykke platform will pass atomic public address
        /// consisted of the main and extension parts separated
        /// by this character where applicable. Extension part
        /// can be omitted, for example to represent Hot Wallet
        /// address, if applicable.
        /// Example:
        ///   separator: ‘$’
        ///   main public address: “Zgu2QfU9PDyvySPm”
        ///   public address extension: “180468”
        ///   atomic public address: “Zgu2QfU9PDyvySPm$180468”
        /// </summary>
        public char Separator { get; }

        /// <summary>
        /// Public address extension part name, which will
        /// displayed to the user whenever public address
        /// will be displayed or entered by the client.
        /// </summary>
        public string DisplayName { get; }

        public PublicAddressExtensionConstants(PublicAddressExtensionConstantsContract contract)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Transaction not found");
            }
            if (char.IsControl(contract.Separator)) 
            {
                throw new ResultValidationException("Separator can't be control character", (int)contract.Separator);
            }
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                throw new ResultValidationException("Display name can't be empty", contract.DisplayName);
            }

            Separator = contract.Separator;
            DisplayName = contract.DisplayName;
        }
    }
}
