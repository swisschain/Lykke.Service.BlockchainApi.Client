using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Responses
{
    [PublicAPI]
    public class AssetsListResponse
    {
        [JsonProperty("assets")]
        public IReadOnlyList<AssetResponse> Assets { get; set; }
    }
}