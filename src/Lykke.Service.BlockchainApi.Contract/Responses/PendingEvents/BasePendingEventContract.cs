using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents
{
    [PublicAPI]
    public class BasePendingEventContract
    {
        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}