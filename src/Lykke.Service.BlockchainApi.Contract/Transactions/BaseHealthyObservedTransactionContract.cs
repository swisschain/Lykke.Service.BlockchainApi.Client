using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base healthy (not failed) observed transaction contract
    /// </summary>
    [PublicAPI]
    public abstract class BaseHealthyObservedTransactionContract : BaseObservedTransactionContract
    {
        /// <summary>
        /// Transaction hash as base64 string.
        /// Can be empty
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
