using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base observed transaction contract
    /// </summary>
    [PublicAPI]
    public abstract class BaseObservedTransactionContract : BaseTransactionContract
    {
        /// <summary>
        /// Fee. Is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// </summary>
        [JsonProperty("fee")]
        public string Fee { get; set; }
    }
}
