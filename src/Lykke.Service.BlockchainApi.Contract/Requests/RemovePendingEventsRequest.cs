using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Requests
{
    [PublicAPI]
    public class RemovePendingEventsRequest
    {
        [JsonProperty("operationIds")]
        public IReadOnlyList<Guid> OperationIds { get; set; }
    }
}
