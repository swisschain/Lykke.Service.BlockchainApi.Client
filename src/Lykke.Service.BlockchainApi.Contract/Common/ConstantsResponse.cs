using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Common
{
    /// <summary>
    /// Blockchain constants contract.
    /// Response for the:
    ///     [GET] /api/constants
    /// </summary>
    [PublicAPI]
    public class ConstantsResponse
    {
        /// <summary>
        /// If blockchain requires additional field to represent
        /// public address to use it as a deposit destination, 
        /// then this section should be non empty.
        /// See isPublicAddressExtensionRequired in the 
        /// [GET] /api/capabilities response.
        /// Can be empty.
        /// </summary>
        [CanBeNull]
        [JsonProperty("publicAddressExtension")]
        public PublicAddressExtensionConstantsContract PublicAddressExtension { get; set; }
    }
}
