using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Building parameters for the transaction with many inputs
    /// Request body for the:
    /// - [POST] /api/transactions/many-inputs
    ///     Errors:
    ///         - 501 Not Implemented - function is not implemented in the blockchain.
    /// </summary>
    /// <remarks>
    /// Service should build not signed transaction. 
    /// If transaction with the specified operationId already was built,
    /// it should be ignored and regular response should be returned.
    /// Fee should be included in the specified amount.
    /// </remarks>
    [PublicAPI]
    public class BuildTransactionWithManyInputsRequest
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Sources
        /// </summary>
        [JsonProperty("inputs")]
        public IReadOnlyList<TransactionInputContract> Inputs { get; set; }

        /// <summary>
        /// Destination address
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Asset ID to transfer
        /// </summary>
        [JsonProperty("assetId")]
        public string AssetId { get; set; }
    }
}
