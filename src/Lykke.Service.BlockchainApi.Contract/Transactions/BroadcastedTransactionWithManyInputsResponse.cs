using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Broadcasted transaction with many inputs contract
    /// Response for the:
    /// - [GET] /api/transactions/broadcast/many-inputs/{operationId}
    ///     Errors:
    ///         - 501 Not Implemented - function is not implemented in the blockchain.
    ///         - 204 No content - specified transaction not found
    /// </summary>
    [PublicAPI]
    public class BroadcastedTransactionWithManyInputsResponse : BaseBroadcastedTransactionResponse
    {
        /// <summary>
        /// Sources
        /// </summary>
        [JsonProperty("inputs")]
        public IReadOnlyList<TransactionInputContract> Inputs { get; set; }
    }
}
