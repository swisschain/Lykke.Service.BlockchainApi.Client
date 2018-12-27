using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Wallets;

namespace Lykke.Service.BlockchainApi.Sdk
{
    public interface IBlockchainSignService
    {
        Task<WalletResponse> CreateWalletAsync();
        Task<(string hash, string signedTransaction)> SignTransactionAsync(string transactionContext, IReadOnlyList<string> privateKeys);
        bool ValidatePrivateKey(string key);
        bool ValidateHotWalletPrivateKey(string key);
    }
}