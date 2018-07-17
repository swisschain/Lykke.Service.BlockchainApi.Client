using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base class for transaction output
    /// </summary>
    [PublicAPI]
    public abstract class BaseTransactionOutputContract
    {
        /// <summary>
        /// Destination address
        /// 
        /// For the blockchains with address mapping,
        /// this could be virtual or underlying address.
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Amount to transfer to the <see cref="ToAddress"/> or actual amount, 
        /// which is transferred to the <see cref="ToAddress"/>, depending on the context.
        /// Integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
