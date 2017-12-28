using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses
{
    [PublicAPI]
    public class PendingEventsResponse<TEvent>
    {
        [JsonProperty("events")]
        public IReadOnlyList<TEvent> Events { get; set; }
    }
}