using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Addresses
{
    /// <summary>
    /// Address validation result.
    /// Response for the:
    /// - [GET] /api/addresses/{address}/validity
    /// </summary>
    [PublicAPI]
    public class AddressValidationResponse
    {
        /// <summary>
        /// Flag, which indicates is address valid
        /// </summary>
        [JsonProperty("isValid")]
        public bool IsValid { get; set; }
    }
}
