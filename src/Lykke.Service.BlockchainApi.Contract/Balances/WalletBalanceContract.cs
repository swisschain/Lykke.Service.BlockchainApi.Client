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
        /// 
        /// For the blockchains with address mapping,
        /// this should be virtual address
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

        /// <summary>
        /// Incremental ID of the moment, when balance
        /// is updated. It should be the same sequence
        /// as for block in the 
        /// [GET] /api/transactions/broadcast/* response
        /// For the most blockchains it could be the 
        /// block number/height
        /// </summary>
        [JsonProperty("block")]
        public long Block { get; set; }

        /// <summary>
        /// Flag that indicate, if given address is 
        /// compromised and can’t be used for further 
        /// for input transactions.
        /// Optional. If omitted, will be interpreted as
        /// false
        /// </summary>
        [CanBeNull]
        [JsonProperty("isAddressCompromised")]
        public bool? IsAddressCompromised { get; set; }
    }
}
