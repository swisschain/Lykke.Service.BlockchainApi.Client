using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction, that should be signed.
    /// Request to [POST] /api/sign
    /// </summary>
    [PublicAPI]
    public class SignTransactionRequest
    {
        /// <summary>
        /// Private keys, which were returned by the [POST] /api/wallets.
        /// Multiple keys can be used for transactions with multiple inputs.
        /// </summary>
        [JsonProperty("privateKeys")]
        public IReadOnlyList<string> PrivateKeys { get; set; }

        /// <summary>
        /// The transaction context in the blockchain specific format, 
        /// which was returned by the Blockchain.Api [POST] /api/transactions or [PUT] /api/transactions.
        /// </summary>
        [JsonProperty("transactionContext")]
        public string TransactionContext { get; set; }
    }
}
