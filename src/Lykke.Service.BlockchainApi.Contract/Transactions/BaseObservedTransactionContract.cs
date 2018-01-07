using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base observed transaction contract
    /// </summary>
    [PublicAPI]
    public abstract class BaseObservedTransactionContract
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Transaction moment as ISO 8601 in UTC
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Source address
        /// </summary>
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination address
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Asset ID
        /// </summary>
        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        /// <summary>
        /// Amount. Is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// calculated as 
        /// x = sourceAmount * (10 ^ asset.Accuracy)
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
