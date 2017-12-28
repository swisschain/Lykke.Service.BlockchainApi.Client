using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents
{
    [PublicAPI]
    public class PendingCashoutFailedEventContract : BasePendingEventContract
    {
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }
    }
}