using System;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Blockchain API health probe
    /// </summary>
    [PublicAPI]
    public class BlockchainHealthProbe
    {
        /// <summary>
        /// Blockchain API name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Blockchain API version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Blockchain API ENV_INFO
        /// </summary>
        public string Env { get; set; }

        /// <summary>
        /// Blockchain API debug build flag
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Contract version supported by the blockchain API
        /// </summary>
        public Version ContractVersion { get; set; }
    }
}
