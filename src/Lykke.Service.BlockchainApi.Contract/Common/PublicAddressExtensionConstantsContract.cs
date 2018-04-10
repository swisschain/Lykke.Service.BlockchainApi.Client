using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Common
{
    /// <summary>
    /// Blockchain public address extension constants contract.
    /// Part of the <see cref="CapabilitiesResponse"/>
    /// </summary>
    [PublicAPI]
    public class PublicAddressExtensionConstantsContract
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
        [JsonProperty("separator")]
        public char Separator { get; set; }

        /// <summary>
        /// Public address extension part name, which will
        /// displayed to the user whenever public address
        /// will be displayed or entered by the client.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Public address main part name, which will
        /// displayed to the user whenever public address
        /// will be displayed or entered by the client.
        /// If this field is empty, then default name
        /// will be used.
        /// Can be Empty.
        /// </summary>
        [JsonProperty("baseDisplayName")]
        public string BaseDisplayName { get; set; }
    }
}
