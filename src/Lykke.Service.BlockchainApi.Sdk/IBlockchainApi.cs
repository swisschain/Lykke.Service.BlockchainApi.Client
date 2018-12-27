using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Common;

namespace Lykke.Service.BlockchainApi.Sdk
{
    public interface IBlockchainApi
    {
        CapabilitiesResponse GetCapabilities();

        ConstantsResponse GetConstants();
        
        string[] GetExplorerUrl(string address);
        
        bool AddressIsValid(string address);
        
        Task<bool> AddressIsExistAsync(string address);
        
        Task<long> GetLastConfirmedBlockNumberAsync();

        bool CanGetBalances { get; }

        Task<BlockchainBalance[]> GetBalancesAsync(string[] addresses, Func<string, Task<IAsset>> getAsset);
        
        Task<(string transactionContext, decimal fee, long expiration)> BuildTransactionAsync(Guid operationId, IAsset asset, IReadOnlyList<IOperationAction> actions, bool includeFee);
        
        Task<string> BroadcastTransactionAsync(string signedTransaction);

        Task<BlockchainTransaction> GetTransactionAsync(string transactionHash, long expiration, IAsset asset);

        Task ObserveAddressAsync(string address);

        Task DeleteAddressObservationAsync(string address);
    }
}