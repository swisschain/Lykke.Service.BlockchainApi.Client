using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction broadcasting parameters.
    /// Request body for the:
    /// - [POST] /api/transactions/broadcast
    ///     Errors:
    ///         - 409 Conflict: transaction with specified operationId is already broadcasted.
    /// </summary>
    /// <remarks>
    /// Service should broadcast the signed transaction and start to observe its execution
    /// </remarks>
    [PublicAPI]
    public class BroadcastTransactionRequest
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        /// <summary>
        /// The signed transaction returned by the
        /// Blockchain.SignService [POST] /api/sign
        /// </summary>
        [JsonProperty("signedTransaction")]
        public string SignedTransaction { get; set; }
    }
}
