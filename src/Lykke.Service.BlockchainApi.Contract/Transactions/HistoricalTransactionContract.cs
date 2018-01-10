using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Historical transaction contract
    /// </summary>
    [PublicAPI]
    public class HistoricalTransactionContract : BaseTransactionContract
    {
        /// <summary>
        /// Transaction hash as base64 string.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
