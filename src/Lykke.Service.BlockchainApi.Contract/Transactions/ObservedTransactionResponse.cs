using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base observed transaction contract
    /// Response for the:
    /// - [GET] /api/transactions/{operationId}
    ///     Errors:
    ///         - 204 No Content: specified transaction not found.
    /// </summary>
    [PublicAPI]
    public class ObservedTransactionResponse
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        /// <summary>
        /// State of the transaction
        /// </summary>
        [JsonProperty("state")]
        public ObservedTransactionState State { get; set; }

        /// <summary>
        /// Transaction moment as ISO 8601 in UTC
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Amount without fee. Is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Fee. Is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// </summary>
        [JsonProperty("fee")]
        public string Fee { get; set; }

        /// <summary>
        /// Transaction hash as base64 string.
        /// Can be empty.
        /// Should be non empty if the <see cref="State"/> is <see cref="ObservedTransactionState.InProgress"/>
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Error description.
        /// Can be empty.
        /// Should be non empty if the <see cref="State"/> is <see cref="ObservedTransactionState.Failed"/>
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
