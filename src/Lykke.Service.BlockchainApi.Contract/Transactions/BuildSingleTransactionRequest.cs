using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction building parameters
    /// Request body for the:
    /// - [POST] /api/transactions
    /// </summary>
    /// <remarks>
    /// Service should build not signed transaction. 
    /// If transaction with the specified operationId already was built,
    /// it should be ignored and regular response should be returned
    /// </remarks>
    [PublicAPI]
    public class BuildSingleTransactionRequest
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
        /// Destination address
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Asset ID to transfer
        /// </summary>
        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        /// <summary>
        /// Amount to transfer. Integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Flag, which indicates, that fee should be included
        /// in the specified amount.
        /// Example: 
        /// if(includeFee == true) actualAmount = amount
        /// if(includeFee == false) actualAmount = amount + fee
        /// </summary>
        [JsonProperty("includeFee")]
        public bool IncludeFee { get; set; }
    }
}
