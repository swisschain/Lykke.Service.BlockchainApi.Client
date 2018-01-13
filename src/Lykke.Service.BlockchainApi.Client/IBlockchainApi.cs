using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract;
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
        /// Should return batch blockchain assets (coins, tags). If there are no assets, 
        /// empty array should be returned. Amount of the returned assets should not exceed <paramref name="take"/>.
        /// Optional <paramref name="continuation"/> contains context of the previous request, to let Blockchain.Api
        /// resume reading of the assets from the previous position. If <paramref name="continuation"/> is empty, assets 
        /// should be read from the beginning.
        /// </summary>
        /// <param name="take">Maximum wallets to return</param>
        /// <param name="continuation">Continuation token returned by the previous request, or null</param>
        [Get("/api/assets")]
        Task<PaginationResponse<AssetContract>> GetAssetsAsync(int take, string continuation);

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
        /// Wallets balance observation is enabled by the 
        /// <see cref="StartBalanceObservationAsync"/> and disabled by the <see cref="StopBalanceObservationAsync"/>.
        /// If there are no balances to return, empty array should be returned.
        /// Amount of the returned wallets should not exceed <paramref name="take"/>. 
        /// Optional continuation contains context of the previous request, to let Blockchain.Api 
        /// resume reading of the balances from the previous position.
        /// If continuation is empty, balances should be read from the beginning.
        /// </summary>
        /// <param name="take">Maximum wallets to return</param>
        /// <param name="continuation">Continuation token returned by the previous request, or null</param>
        [Get("/api/balances")]
        Task<PaginationResponse<WalletBalanceContract>> GetWalletBalancesAsync(int take, string continuation);

        #endregion


        #region Transactions

        /// <summary>
        /// Should build not signed transaction. If transaction with the specified 
        /// <see cref="BroadcastTransactionRequest.OperationId"/> already was built, 
        /// it should be ignored and regular response should be returned.
        /// 
        /// Errors:
        /// - 406 Not Acceptable: transaction can’t be built due to non acceptable amount (too small for example).
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
        /// to try to execute transaction with higher fee. [POST] /api/transactions with the same 
        /// operationId should precede to the given call. Transaction should be rebuilt with 
        /// parameters that were passed to the [POST] /api/transactions.
        /// 
        /// Errors:
        /// - 406 Not Acceptable: transaction can’t be built due to non acceptable amount (too small for example).
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
        /// Should return observed transaction be the operationId. Transaction observation is started when t
        /// he transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>
        /// 
        /// Errors:
        /// - 204 No content - specified transaction not found
        /// </summary>
        [Get("/api/transactions/observed/{operationId}")]
        Task<ObservedTransactionResponse> GetObservedTransactionAsync(Guid operationId);

        /// <summary>
        /// Should stop observation of the specified transactions. 
        /// If one or many of the specified transactions not found in the observed transactions, 
        /// they should be ignored. Should affect transactions list returned by the
        /// <see cref="GetObservedTransactionAsync"/>
        /// </summary>
        [Delete("/api/transactions/observation")]
        Task StopTransactionsObservationAsync([Body] IReadOnlyList<Guid> body);

        /// <summary>
        /// Should start observation of the transactions that transfer fund from the address. 
        /// Should affect result of the [GET] /api/transactions/history/from/{address}.
        /// 
        /// Errors:
        /// - 409 Conflict: transactions from the address are already observed.
        /// </summary>
        /// <param name="address">Address for which outgoing transactions history should be observed</param>
        [Post("/api/transactions/history/from/{address}/observation")]
        Task StartHistoryObservationOfOutgoingTransactionsAsync(string address);

        /// <summary>
        /// Should start observation of the transactions that transfer fund to the address. 
        /// Should affect result of the [GET] /api/transactions/history/to/{address}.
        /// 
        /// Errors:
        /// - 409 Conflict: transactions to the address are already observed.
        /// </summary>
        /// <param name="address">Address for which incoming transactions history should be observed</param>
        [Post("/api/transactions/history/from/{address}/observation")]
        Task StartHistoryObservationOfIncomingTransactions(string address);

        /// <summary>
        /// Should return completed transactions that transfer fund from the <paramref name="address"/> and that 
        /// were broadcasted after the transaction with the hash equal to the <paramref name="afterHash"/>.
        /// Should include transactions broadcasted not using this API.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// </summary>
        /// <param name="address">Address for which outgoing transactions history should be returned</param>
        /// <param name="afterHash">Hash of the transaction after which history should be returned</param>
        /// <param name="take">Maximum transactions to return</param>
        [Get("/api/transactions/history/from/{address}")]
        Task<IReadOnlyList<HistoricalTransactionContract>> GetHistoryOfOutgoingTransactions(string address, string afterHash, int take);

        /// <summary>
        /// Should return completed transactions that transfer fund to the <paramref name="address"/> and that 
        /// were broadcasted after the transaction with the hash equal to the <paramref name="afterHash"/>.
        /// Should include transactions broadcasted not using this API.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// </summary>
        /// <param name="address">Address for which incoming transactions history should be returned</param>
        /// <param name="afterHash">Hash of the transaction after which history should be returned</param>
        /// <param name="take">Maximum transactions to return</param>
        [Get("/api/transactions/history/to/{address}")]
        Task<IReadOnlyList<HistoricalTransactionContract>> GetHistoryOfIncomingTransactions(string address, string afterHash, int take);

        #endregion
    }
}
