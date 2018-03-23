using System;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.BlockchainApi.Contract
{
    /// <summary>
    /// Response for the:
    ///     [GET] /api/isalive
    /// </summary>
    [PublicAPI]
    public class BlockchainIsAliveResponse : IsAliveResponse
    {
        /// <summary>
        /// Should return implemented contract version.
        /// Can be empty.
        /// Empty value should be treated as “1.0.0”
        /// For example: “1.0.0”
        /// </summary>
        [CanBeNull]
        [JsonProperty("contractVersion")]
        [JsonConverter(typeof(VersionConverter))]
        public Version ContractVersion { get; set; }
    }
}
