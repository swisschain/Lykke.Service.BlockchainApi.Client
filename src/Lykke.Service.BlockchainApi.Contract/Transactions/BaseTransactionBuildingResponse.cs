﻿using JetBrains.Annotations;
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
        /// Error code.
        /// Should be non empty if an error that match one of the
        /// listed code is occured. For other errors use HTTP
        /// status codes.
        /// </summary>
        [JsonProperty("errorCode")]
        public TransactionExecutionError? ErrorCode { get; set; }

        /// <summary>
        /// The transaction context in the blockchain 
        /// specific format, which will be passed to the
        /// Blockchain.SignService [POST] /api/sign
        /// </summary>
        [JsonProperty("transactionContext")]
        public string TransactionContext { get; set; }
    }
}
