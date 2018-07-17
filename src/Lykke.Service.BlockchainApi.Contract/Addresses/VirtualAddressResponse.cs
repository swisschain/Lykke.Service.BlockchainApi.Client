using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Addresses
{
    /// <summary>
    /// Virtual address contract
    /// Used in:
    /// - [GET] /api/addresses/{address}/virtual
    /// </summary>
    [PublicAPI]
    public class VirtualAddressResponse
    {
        /// <summary>
        /// Virtual  address
        /// </summary>
        [JsonProperty("virtualAddress")]
        public string VirtualAddress { get; set; }
    }
}
