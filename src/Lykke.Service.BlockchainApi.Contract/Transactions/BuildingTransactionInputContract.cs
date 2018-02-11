using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction input request contract
    /// </summary>
    [PublicAPI]
    public class BuildingTransactionInputContract : BaseTransactionInputContract
    {
        /// <summary>
        /// Any non security sensitive data associated with
        /// source wallet, that were returned by the
        /// Blockchain.SignService [POST] /api/wallets.
        /// Can be empty.
        /// </summary>
        [JsonProperty("fromAddressContext")]
        public string FromAddressContext { get; set; }
    }
}
