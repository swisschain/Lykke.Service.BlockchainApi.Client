using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Lykke.Service.BlockchainApi.Contract.Common;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Historical transaction contract.
    /// Response for the:
    ///     [get] /api/transactions/history/from/{address}
    ///     [get] /api/transactions/history/to/{address}
    /// </summary>
    [PublicAPI]
    public class HistoricalTransactionContract
    {
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
        /// Amount without fee. Is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Transaction hash as base64 string.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Type of the transaction.
        /// Can be empty.
        /// Should be non empty if the flag
        /// <see cref="CapabilitiesResponse.IsReceiveTransactionRequired"/> is true
        /// </summary>
        [CanBeNull]
        [JsonProperty("transactionType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionType? TransactionType { get; set; }
    }
}
