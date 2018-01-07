using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Base request body for the transaction building.
    /// </summary>
    [PublicAPI]
    public abstract class BaseTransactionBuildingRequest
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
        /// in the specified amount
        /// </summary>
        [JsonProperty("includeFee")]
        public bool IncludeFee { get; set; }
    }
}
