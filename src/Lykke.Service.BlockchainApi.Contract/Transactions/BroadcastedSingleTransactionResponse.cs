using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Broadcasted single transaction contract
    /// Response for the:
    /// - [GET] /api/transactions/broadcast/{operationId}
    ///     Errors:
    ///         - 204 No Content: specified transaction not found.
    /// </summary>
    [PublicAPI]
    public class BroadcastedSingleTransactionResponse : BaseBroadcastedTransactionResponse
    {
        /// <summary>
        /// Amount without fee. Is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// Should be non empty if the <see cref="BaseBroadcastedTransactionResponse.State"/> is <see cref="BroadcastedTransactionState.Completed"/>
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
