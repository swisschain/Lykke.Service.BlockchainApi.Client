using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Observed transaction, that is completed.
    /// Used in the:
    /// - [GET] /api/transactions/completed?take=integer&amp;skip=integer response
    /// </summary>
    [PublicAPI]
    public class CompletedTransactionContract : BaseObservedTransactionContract
    {
        /// <summary>
        /// Transaction hash as base64 string.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
