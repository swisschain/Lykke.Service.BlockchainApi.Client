using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction sign result.
    /// Response for [POST] /api/sign
    /// </summary>
    [PublicAPI]
    public class SignedTransactionResponse
    {
        /// <summary>
        /// Signed transaction, which will be used to broadcast the transaction by the Blockchain.Api
        /// </summary>
        [JsonProperty("signedTransaction")]
        public string SignedTransaction { get; set; }
    }
}
