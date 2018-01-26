using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Broadcasted transaction with many outputs contract
    /// Response for the:
    /// - [GET] /api/transactions/broadcast/many-outputs/{operationId}
    ///     Errors:
    ///         - 501 Not Implemented - function is not implemented in the blockchain.
    ///         - 204 No content - specified transaction not found
    /// </summary>
    [PublicAPI]
    public class BroadcastedTransactionWithManyOutputsResponse : BaseBroadcastedTransactionResponse
    {
        /// <summary>
        /// Destinations
        /// </summary>
        [JsonProperty("outputs")]
        public IReadOnlyList<TransactionOutputContract> Outputs { get; set; }
    }
}
