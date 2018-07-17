using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Addresses
{
    /// <summary>
    /// Underlying (blockchain native) address contract
    /// Used in:
    /// - [GET] /api/addresses/{address}/underlying
    /// </summary>
    [PublicAPI]
    public class UnderlyingAddressResponse
    {
        /// <summary>
        /// Underlying address
        /// </summary>
        [JsonProperty("underlyingAddress")]
        public string UnderlyingAddress { get; set; }
    }
}
