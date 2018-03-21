using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Testing
{
    /// <summary>
    /// Testing transfer request contract.
    /// Request body for the:
    ///     [POST] /api/testing/transfers
    /// </summary>
    [PublicAPI]
    public class TestingTransferRequest
    {
        /// <summary>
        /// Source address 
        /// </summary>
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Source address private key
        /// </summary>
        [JsonProperty("fromPrivateKey")]
        public string FromPrivateKey { get; set; }

        /// <summary>
        /// Destination address
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Asset ID
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
    }
}
