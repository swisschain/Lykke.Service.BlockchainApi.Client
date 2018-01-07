using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base response for the transaction building
    /// </summary>
    [PublicAPI]
    public abstract class BaseTransactionBuildingResponse
    {
        /// <summary>
        /// The transaction context in the blockchain 
        /// specific format, which will be passed to the
        /// Blockchain.SignService [POST] /api/sign
        /// </summary>
        [JsonProperty("transactionContext")]
        public string TransactionContext { get; set; }
    }
}
