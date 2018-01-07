using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Observed transaction, that is failed.
    /// Used in the:
    /// - [GET] /api/transactions/failed?take=integer&amp;skip=integer response
    /// </summary>
    [PublicAPI]
    public class FailedTransactionContract : BaseObservedTransactionContract
    {
        /// <summary>
        /// Error description
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
