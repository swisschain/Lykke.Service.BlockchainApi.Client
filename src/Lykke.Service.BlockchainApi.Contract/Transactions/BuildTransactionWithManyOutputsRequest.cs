using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Building parameters for the transaction with many outputs
    /// Request body for the:
    /// - [POST] /api/transactions/many-outputs
    ///     Errors:
    ///         - 501 Not Implemented - function is not implemented in the blockchain.
    /// </summary>
    /// <remarks>
    /// Service should build not signed transaction. 
    /// If transaction with the specified operationId already was built,
    /// it should be ignored and regular response should be returned.
    /// Fee should be added to the specified amount.
    /// </remarks>
    [PublicAPI]
    public class BuildTransactionWithManyOutputsRequest
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Source address
        /// </summary>
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Any non security sensitive data associated with
        /// source wallet, that were returned by the
        /// Blockchain.SignService [POST] /api/wallets.
        /// Can be empty.
        /// </summary>
        [JsonProperty("fromAddressContext")]
        public string FromAddressContext { get; set; }

        /// <summary>
        /// Destinations
        /// </summary>
        [JsonProperty("outputs")]
        public IReadOnlyList<TransactionOutputContract> Outputs { get; set; }

        /// <summary>
        /// Asset ID to transfer
        /// </summary>
        [JsonProperty("assetId")]
        public string AssetId { get; set; }
    }
}
