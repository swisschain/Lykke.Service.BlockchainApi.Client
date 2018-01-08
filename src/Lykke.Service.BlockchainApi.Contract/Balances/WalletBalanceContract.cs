using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Balances
{
    /// <summary>
    /// Blockchain wallet balance contract.
    /// Used in:
    /// - [GET] /api/balances?take=integer&amp;skip=integer
    /// </summary>
    [PublicAPI]
    public class WalletBalanceContract
    {
        /// <summary>
        /// Wallet address
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// Asset ID
        /// </summary>
        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        /// <summary>
        /// Balance is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// writen using <see cref="Conversions.CoinsToContract"/>
        /// and can be read using <see cref="Conversions.CoinsFromContract"/>
        /// </summary>
        [JsonProperty("balance")]
        public string Balance { get; set; }
    }
}
