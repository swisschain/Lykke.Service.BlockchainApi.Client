using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses
{
    [PublicAPI]
    public class AssetResponse
    {
        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Max number of significant decimal digits to the right of the decimal point in the asset amount
        /// </summary>
        [JsonProperty("accuracy")]
        public int Accuracy { get; set; }
    }
}