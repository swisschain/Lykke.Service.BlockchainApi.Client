using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Wallets;

namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Cryptography API
    /// </summary>
    public interface IBlockchainSignService
    {
        /// <summary>
        /// Returns new blockchain address (account).
        /// For blockchains which support memo-like feature may return hot-wallet address with extension.
        /// </summary>
        /// <returns></returns>
        Task<WalletResponse> CreateWalletAsync();

        /// <summary>
        /// Signs transaction.
        /// </summary>
        /// <param name="transactionContext">Result of <see cref="IBlockchainApi.BuildTransactionAsync()"/> call.</param>
        /// <param name="privateKeys">Array of private keys required to sign the transaction.</param>
        /// <returns></returns>
        Task<(string hash, string signedTransaction)> SignTransactionAsync(string transactionContext, IReadOnlyList<string> privateKeys);

        /// <summary>
        /// Validates private key format.
        /// </summary>
        /// <param name="key">Private key.</param>
        /// <returns></returns>
        bool ValidatePrivateKey(string key);

        /// <summary>
        /// Validates private key if hot wallet.
        /// Is called when signing simulated transaction only.
        /// </summary>
        /// <param name="key">Private key.</param>
        /// <returns></returns>
        bool ValidateHotWalletPrivateKey(string key);
    }
}