using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses
{
    [PublicAPI]
    public class AddressValidationResponse
    {
        [JsonProperty("isValid")]
        public bool IsValid { get; set; }
    }
}