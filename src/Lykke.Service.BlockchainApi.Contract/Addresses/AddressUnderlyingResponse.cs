using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Addresses
{
    /// <summary>
    /// Should return underlying (blockchain native) address 
    /// for the given virtual address.
    /// </summary>
    [PublicAPI]
    public class AddressUnderlyingResponse
    {
        /// <summary>
        /// Underlying address
        /// </summary>
        [JsonProperty("underlyingAddress")]
        public string UnderlyingAddress { get; set; }
    }
}
