using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses
{
    [PublicAPI]
    public class WalletCreationResponse
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}