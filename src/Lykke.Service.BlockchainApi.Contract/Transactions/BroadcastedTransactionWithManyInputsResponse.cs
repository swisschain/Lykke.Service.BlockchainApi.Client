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
        /// Sources.
        /// Should be non null if the <see cref="BaseBroadcastedTransactionResponse.State"/> is <see cref="BroadcastedTransactionState.Completed"/>.
        /// </summary>
        [JsonProperty("inputs")]
        public IReadOnlyList<TransactionInputContract> Inputs { get; set; }
    }
}
