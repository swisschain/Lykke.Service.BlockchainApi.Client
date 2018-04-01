using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base class for the broadcasted transaction contract
    /// </summary>
    [PublicAPI]
    public abstract class BaseBroadcastedTransactionResponse
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
        [JsonConverter(typeof(StringEnumConverter))]
        public BroadcastedTransactionState State { get; set; }

        /// <summary>
        /// Transaction moment as ISO 8601 in UTC
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Fee. Is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// Should be non empty if the <see cref="State"/> is <see cref="BroadcastedTransactionState.Completed"/>
        /// </summary>
        [CanBeNull]
        [JsonProperty("fee")]
        public string Fee { get; set; }

        /// <summary>
        /// Transaction hash as base64 string.
        /// Can be empty.
        /// Should be non empty if the <see cref="State"/> is <see cref="BroadcastedTransactionState.Completed"/>
        /// </summary>
        [CanBeNull]
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Error description.
        /// Can be empty.
        /// Should be non empty if the <see cref="State"/> is <see cref="BroadcastedTransactionState.Failed"/>
        /// </summary>
        [CanBeNull]
        [JsonProperty("error")]
        public string Error { get; set; }

        /// <summary>
        /// Error code.
        /// Should be non empty if the <see cref="State"/> is <see cref="BroadcastedTransactionState.Failed"/>
        /// </summary>
        [CanBeNull]
        [JsonProperty("errorCode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainErrorCode? ErrorCode { get; set; }

        /// <summary>
        /// Incremental ID of the moment, when the transaction
        /// state changing is detected. It should be the same
        /// sequence as for <see cref="WalletBalanceContract.Block"/>. 
        /// For the most blockchains it could be the block number/height.
        /// </summary>
        [JsonProperty("block")]
        public long Block { get; set; }
    }
}
