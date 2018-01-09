using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract.Addresses;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    internal interface IBlockchainApi
    {
        /// <summary>
        /// Should return some general service info. Used to check is service running
        /// </summary>
        [Get("/api/isalive")]
        Task<IsAliveResponse> GetIsAliveAsync();

        #region Assets

        /// <summary>
        /// Should return all blockchain assets (coins, tags). If there are no assets, empty array should be returned
        /// </summary>
        [Get("/api/assets")]
        Task<IReadOnlyList<AssetContract>> GetAssetsAsync();

        /// <summary>
        /// Should return specified asset (coin, tag)
        /// 
        /// Errors:
        /// - 204 No content: specified asset not found
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        [Get("/api/assets/{assetId}")]
        Task<AssetResponse> GetAssetAsync(string assetId);

        #endregion


        #region Addresses

        /// <summary>
        /// Should check and return wallet address validity
        /// </summary>
        /// <param name="address">Wallet address</param>
        [Get("/api/addresses/{address}/validity")]
        Task<AddressValidationResponse> IsAddressValidAsync(string address);

        #endregion


        #region Balances

        /// <summary>
        /// Should remember the wallet address to observe the wallet balance and return it in the 
        /// <see cref="GetWalletBalancesAsync"/>, if the balance is non zero.
        /// 
        /// Errors:
        /// - 409 Conflict: specified address is already observed.
        /// </summary>
        /// <param name="address">Wallet address</param>
        [Post("/api/balances/{address}/observation")]
        Task StartBalanceObservationAsync(string address);

        /// <summary>
        /// Should forget the wallet address and stop observe its balance.
        /// 
        /// Errors:
        /// - 204 No content: specified address is not observed
        /// </summary>
        /// <param name="address">Wallet address</param>
        [Delete("/api/balances/{address}/observation")]
        Task StopBalanceObservationAsync(string address);

        /// <summary>
        /// Should return balances of the observed wallets with non zero balances.
        /// Wallets balance observation is enabled by the <see cref="StartBalanceObservationAsync"/> and 
        /// disabled by the <see cref="StopBalanceObservationAsync"/>.
        /// If there are no balances to return, empty array should be returned.
        /// Amount of the returned wallets should not exceed <paramref name="take"/>.
        /// <paramref name="skip"/> balances should be skipped before return first balance.
        /// </summary>
        /// <param name="take">Maximum wallets to return</param>
        /// <param name="skip">Amount of the wallets to skip before return first wallet</param>
        [Get("/api/balances")]
        Task<IReadOnlyList<WalletBalanceContract>> GetWalletBalancesAsync(int take, int skip);

        #endregion


        #region Transactions

        /// <summary>
        /// Should build not signed transaction. If transaction with the specified 
        /// <see cref="BroadcastTransactionRequest.OperationId"/> already was built, 
        /// it should be ignored and regular response should be returned.
        /// </summary>
        [Post("/api/transactions")]
        Task<BuildTransactionResponse> BuildTransactionAsync([Body] BuildTransactionRequest body);

        /// <summary>
        /// Optional method.
        /// 
        /// Should rebuild not signed transaction with the specified fee factor, 
        /// if applicable for the given blockchain. This should be implemented, 
        /// if blockchain allows transaction rebuilding (substitution) with new fee. 
        /// This will be called if transaction is stuck in the “in-progress” state for too long,
        /// to try to execute transaction with higher fee.
        /// </summary>
        [Put("/api/transactions")]
        Task<RebuildTransactionResponse> RebuildTransactionAsync([Body] RebuildTransactionRequest body);

        /// <summary>
        /// Should broadcast the signed transaction and start to observe its execution.
        /// 
        /// Errors:
        /// - 409 Conflict: transaction with specified operationId and signedTransaction is already broadcasted.
        /// </summary>
        [Post("/api/transactions/broadcast")]
        Task BroadcastTransactionAsync([Body] BroadcastTransactionRequest body);

        /// <summary>
        /// Should return observed transactions being in progress. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// Transaction should be removed from this collection when its state is changed to the completed or failed
        /// <paramref name="skip"/> transactions should be skipped before return first transaction.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="skip">Amount of the transactions to skip before return first transaction</param>
        [Get("/api/transactions/completed")]
        Task<IReadOnlyList<InProgressTransactionContract>> GetInProgressTransactionsAsync(int take, int skip);

        /// <summary>
        /// Should return completed observed transactions. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// <paramref name="skip"/> transactions should be skipped before return first transaction.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="skip">Amount of the transactions to skip before return first transaction</param>
        [Get("/api/transactions/in-progress")]
        Task<IReadOnlyList<CompletedTransactionContract>> GetCompletedTransactionsAsync(int take, int skip);

        /// <summary>
        /// Should return failed observed transactions. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// <paramref name="skip"/> transactions should be skipped before return first transaction.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="skip">Amount of the transactions to skip before return first transaction</param>
        [Get("/api/transactions/failed")]
        Task<IReadOnlyList<FailedTransactionContract>> GetFailedTransactionsAsync(int take, int skip);

        /// <summary>
        /// Should stop observation of the specified transactions. 
        /// If one or many of the specified transactions not found in the observed transactions, 
        /// they should be ignored. Should affect transactions list returned by the
        /// <see cref="GetCompletedTransactionsAsync"/> and <see cref="GetFailedTransactionsAsync"/>
        /// </summary>
        [Delete("/api/transactions/observation")]
        Task StopTransactionsObservationAsync([Body] IReadOnlyList<Guid> body);
        
        #endregion
    }
}
