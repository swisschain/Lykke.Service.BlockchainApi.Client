using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Observed transaction, being in the progress.
    /// Used in the:
    /// - [GET] /api/transactions/in-progress?take=integer&amp;skip=integer response
    /// </summary>
    [PublicAPI]
    public class InProgressTransactionContract : BaseObservedTransactionContract
    {
        /// <summary>
        /// Transaction hash as base64 string.
        /// Can be empty
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
