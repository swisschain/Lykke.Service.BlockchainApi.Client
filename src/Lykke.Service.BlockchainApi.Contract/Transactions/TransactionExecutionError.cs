using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transacction execution error
    /// </summary>
    [PublicAPI]
    public enum TransactionExecutionError
    {
        /// <summary>
        /// Any error that does not fit another codes
        /// </summary>
        [JsonProperty("unknown")]
        Unknown,

        /// <summary>
        /// Amount is too small to execute the transaction
        /// </summary>
        [JsonProperty("amountIsTooSmall")]
        AmountIsTooSmall,

        /// <summary>
        /// Transaction can’t be executed due to balance insufficiency on the source address
        /// </summary>
        [JsonProperty("notEnoughBalance")]
        NotEnoughtBalance
    }
}
