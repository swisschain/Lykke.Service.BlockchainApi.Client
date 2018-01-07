using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Assets
{
    /// <summary>
    /// Blockchain asset contract.
    /// Used in:
    /// - [GET] /api/assets response
    /// </summary>
    [PublicAPI]
    public class AssetContract
    {
        /// <summary>
        /// Asset ID
        /// </summary>
        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        /// <summary>
        /// Asset address, which identifies 
        /// asset in the blockchain, if applicable for the given blockchain.
        /// Can be empty.
        /// </summary>
        [JsonProperty("address")]
        [CanBeNull]
        public string Address { get; set; }

        /// <summary>
        /// Asset display name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Asset accuracy - maximum number
        /// of significant decimal digits to the right 
        /// of the decimal point in the asset amount.
        /// Valid range: [1..28]
        /// </summary>
        [JsonProperty("accuracy")]
        public int Accuracy { get; set; }
    }
}
