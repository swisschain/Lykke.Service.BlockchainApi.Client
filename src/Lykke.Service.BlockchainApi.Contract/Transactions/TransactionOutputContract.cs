using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    public class TransactionOutputContract
    {
        /// <summary>
        /// Destination address
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
