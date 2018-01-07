using JetBrains.Annotations;

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
        public string Address { get; set; }

        /// <summary>
        /// Balance is integer as string, aligned 
        /// to the asset accuracy. Actual value can be 
        /// calculated as 
        /// x = sourceBalance * (10 ^ asset.Accuracy)
        /// </summary>
        public string Balance { get; set; }
    }
}
