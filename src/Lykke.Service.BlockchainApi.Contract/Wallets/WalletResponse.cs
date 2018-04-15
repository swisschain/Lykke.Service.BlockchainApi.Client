using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Wallets
{
    /// <summary>
    /// Wallet creation result.
    /// Response for [POST] /api/wallets
    /// </summary>
    [PublicAPI]
    public class WalletResponse
    {
        /// <summary>
        /// Any non security sensitive data associated with wallet. 
        /// This context will be passed to the Blockchain.API [POST] /api/transactions/*.
        /// </summary>
        /// <remarks>
        /// Can be empty.
        /// </remarks>
        [JsonProperty("addressContext"), CanBeNull]
        public string AddressContext { get; set; }

        /// <summary>
        /// Private key, which will be used by Lykke platform to
        /// sign transactions by the [POST] /api/sign
        /// </summary>
        [JsonProperty("privateKey")]
        public string PrivateKey { get; set; }

        /// <summary>
        /// Address which identifies the wallet in the blockchain
        /// </summary>
        [JsonProperty("publicAddress")]
        public string PublicAddress { get; set; }
    }
}
