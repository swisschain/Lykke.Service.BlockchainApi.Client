using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents
{
    [PublicAPI]
    public class PendingCashinEventContract : BasePendingEventContract
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
