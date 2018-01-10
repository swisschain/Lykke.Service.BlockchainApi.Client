using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Models;

namespace Lykke.Service.BlockchainApi.Client
{
    [PublicAPI]
    public interface IBlockchainApiClient : IDisposable
    {
        string HostUrl { get; }

        #region General

        /// <summary>
        /// Should return some general service info. Used to check is service running
        /// </summary>
        Task<IsAliveResponse> GetIsAliveAsync();

        #endregion


        #region Assets

        /// <summary>
        /// Should return all blockchain assets (coins, tags). If there are no assets, empty array should be returned
        /// </summary>
        Task<PaginationResult<BlockchainAsset>> GetAssetsAsync(int take, string continuation);

        /// <summary>
        /// Should return specified asset (coin, tag)
        /// 
        /// Errors:
        /// - 204 No content: specified asset not found
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        Task<BlockchainAsset> GetAssetAsync(string assetId);

        #endregion


        #region Addresses

        /// <summary>
        /// Should check and return wallet address validity
        /// </summary>
        /// <param name="address">Wallet address</param>
        Task<bool> IsAddressValidAsync(string address);

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
        /// <returns>true - if balance observation is started. false - if balance observation was already started</returns>
        Task<bool> StartBalanceObservationAsync(string address);

        /// <summary>
        /// Should forget the wallet address and stop observe its balance.
        /// 
        /// Errors:
        /// - 204 No content: specified address is not observed
        /// </summary>
        /// <param name="address">Wallet address</param>
        /// <returns>true - if balance observation is stopped. false - if balance is not observed to stop it</returns>
        Task<bool> StopBalanceObservationAsync(string address);

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
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<PaginationResult<WalletBalance>> GetWalletBalancesAsync(int take, string continuation, Func<string, int> assetAccuracyProvider);

        #endregion


        #region Transactions

        /// <summary>
        /// Should build not signed transaction. If transaction with the specified 
        /// <paramref name="operationId"/> already was built, 
        /// it should be ignored and regular response should be returned.
        /// </summary>
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="fromAddress">Source address</param>
        /// <param name="toAddress">Destination address</param>
        /// <param name="asset">Blockchain asset to transfer</param>
        /// <param name="amount">Amount to transfer</param>
        /// <param name="includeFee">Flag, which indicates, that fee should be included in the specified amount</param>
        /// <exception cref="NonAcceptableAmountException">
        /// Tranaction <paramref name="amount"/> is non acceptable.
        /// Transaction building should be retried with different <paramref name="amount"/>
        /// </exception>
        Task<TransactionBuildingResult> BuildTransactionAsync(Guid operationId, string fromAddress, string toAddress, BlockchainAsset asset, decimal amount, bool includeFee);

        /// <summary>
        /// Optional method.
        /// 
        /// Should rebuild not signed transaction with the specified fee factor, 
        /// if applicable for the given blockchain. This should be implemented, 
        /// if blockchain allows transaction rebuilding (substitution) with new fee. 
        /// This will be called if transaction is stuck in the “in-progress” state for too long,
        /// to try to execute transaction with higher fee. <see cref="BuildTransactionAsync"/> with 
        /// the same <paramref name="operationId"/> should precede to the given call. 
        /// Transaction should be rebuilt with parameters that were passed to the <see cref="BuildTransactionAsync"/>.
        /// </summary>
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="feeFactor">Multiplier for the transaction fee. Blockchain will multiply regular fee by this factor</param>
        /// <exception cref="NonAcceptableAmountException">
        /// Tranaction <paramref name="amount"/> is non acceptable.
        /// Transaction building should be retried with different <paramref name="amount"/>
        /// </exception>
        Task<TransactionBuildingResult> RebuildTransactionAsync(Guid operationId, decimal feeFactor);

        /// <summary>
        /// Should broadcast the signed transaction and start to observe its execution.
        /// 
        /// Errors:
        /// - 409 Conflict: transaction with specified operationId and signedTransaction is already broadcasted.
        /// </summary>
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="signedTransaction">The signed transaction returned by the Blockchain.SignService after signing</param>
        /// <returns>
        /// true - if transaction is broadcasted. false - if transaction with given <paramref name="operationId"/> 
        /// and <paramref name="signedTransaction"/> was already broadcasted
        /// </returns>
        Task<bool> BroadcastTransactionAsync(Guid operationId, string signedTransaction);

        /// <summary>
        /// Should return observed transactions being in progress. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// Optional <paramref name="continuation"/> contains context of the previous request, to let Blockchain.Api
        /// resume reading of the transactions from the previous position. If <paramref name="continuation"/> is empty, 
        /// transactions should be read from the beginning.
        /// Transaction should be removed from this collection when its state is changed to the completed or failed.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="continuation">Continuation token returned by the previous request, or null</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<PaginationResult<InProgressTransaction>> GetInProgressTransactionsAsync(int take, string continuation, Func<string, int> assetAccuracyProvider);

        /// <summary>
        /// Should return completed observed transactions. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// Optional <paramref name="continuation"/> contains context of the previous request, to let Blockchain.Api
        /// resume reading of the transactions from the previous position. If <paramref name="continuation"/> is empty, 
        /// transactions should be read from the beginning.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="continuation">Continuation token returned by the previous request, or null</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<PaginationResult<CompletedTransaction>> GetCompletedTransactionsAsync(int take, string continuation, Func<string, int> assetAccuracyProvider);

        /// <summary>
        /// Should return failed observed transactions. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// Optional <paramref name="continuation"/> contains context of the previous request, to let Blockchain.Api
        /// resume reading of the transactions from the previous position. If <paramref name="continuation"/> is empty, 
        /// transactions should be read from the beginning.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="continuation">Continuation token returned by the previous request, or null</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<PaginationResult<FailedTransaction>> GetFailedTransactionsAsync(int take, string continuation, Func<string, int> assetAccuracyProvider);

        /// <summary>
        /// Should stop observation of the specified transactions. 
        /// If one or many of the specified transactions not found in the observed transactions, 
        /// they should be ignored. Should affect transactions list returned by the
        /// <see cref="GetCompletedTransactionsAsync"/> and <see cref="GetFailedTransactionsAsync"/>
        /// </summary>
        Task StopTransactionsObservationAsync(IReadOnlyList<Guid> operationIds);

        /// <summary>
        /// Should start observation of the transactions that transfer fund from the address. 
        /// Should affect result of the [GET] /api/transactions/history/from/{address}.
        /// </summary>
        /// <param name="address">Address for which outgoing transactions history should be observed</param>
        /// <returns>
        /// true - if transactions observation is started. 
        /// false - if transactions observation was already started fot the given <paramref name="address"/>
        /// </returns>
        Task<bool> StartHistoryObservationOfOutgoingTransactionsAsync(string address);

        /// <summary>
        /// Should start observation of the transactions that transfer fund to the address. 
        /// Should affect result of the <see cref="GetHistoryOfIncomingTransactions"/>.
        /// </summary>
        /// <param name="address">Address for which incoming transactions history should be observed</param>
        /// <returns>
        /// true - if transactions observation is started. 
        /// false - if transactions observation was already started fot the given <paramref name="address"/>
        /// </returns>
        Task<bool> StartHistoryObservationOfIncomingTransactions(string address);

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
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<IEnumerable<HistoricalTransaction>> GetHistoryOfOutgoingTransactions(string address, string afterHash, int take, Func<string, int> assetAccuracyProvider);

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
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<IEnumerable<HistoricalTransaction>> GetHistoryOfIncomingTransactions(string address, string afterHash, int take, Func<string, int> assetAccuracyProvider);


        #endregion
    }
}
