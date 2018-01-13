using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// State of the observed transaction
    /// </summary>
    public enum ObservedTransactionState
    {
        /// <summary>
        /// Transaction is being in-progress
        /// </summary>
        [JsonProperty("inProgress")]
        InProgress,

        /// <summary>
        /// Transaction is completed for sure
        /// </summary>
        [JsonProperty("completed")]
        Completed,

        /// <summary>
        /// transaction is failed, if applicable for the particular blockchain
        /// </summary>
        [JsonProperty("failed")]
        Failed
    }
}
