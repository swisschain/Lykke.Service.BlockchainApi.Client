using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction type.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Send transaction
        /// </summary>
        [JsonProperty("send")]
        Send,

        /// <summary>
        /// Receive transaction
        /// </summary>
        [JsonProperty("receive")]
        Receive
    }
}
