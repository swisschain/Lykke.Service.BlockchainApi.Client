using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses
{
    [PublicAPI]
    public class PendingEventsResponse<TEvent>
        where TEvent : BasePendingEventContract
    {
        [JsonProperty("events")]
        public IReadOnlyList<TEvent> Events { get; set; }
    }
}
