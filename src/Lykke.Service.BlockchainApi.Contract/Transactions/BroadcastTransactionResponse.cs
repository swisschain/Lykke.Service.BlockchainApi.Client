using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction broadcasting result contract.
    /// Response for the:
    /// - [POST] /api/transactions/broadcast
    ///     Errors:
    ///         - 409 Conflict - transaction with specified operationId and signedTransaction has already been broadcasted.
    /// </summary>
    [PublicAPI]
    public class BroadcastTransactionResponse
    {
        /// <summary>
        /// Error code.
        /// Should be non empty if an error that match one of the
        /// listed code is occured. For other errors use HTTP
        /// status codes.
        /// </summary>
        [JsonProperty("errorCode")]
        public TransactionExecutionError? ErrorCode { get; set; }
    }
}
