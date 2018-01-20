using System;
using System.Collections.Generic;
using System.Net;
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
        /// Enumerates all blockchain assets (coins, tags). To the <paramref name="enumerationCallback"/>
        /// </summary>
        /// <param name="batchSize">Batch size that single request to the Blockchain.Api can return</param>
        /// <param name="enumerationCallback">Enumeration callback, which will be called for every read asset</param>
        Task<EnumerationStatistics> EnumerateAllAssetsAsync(int batchSize, Action<BlockchainAsset> enumerationCallback);

        /// <summary>
        /// Enumerates all blockchain assets (coins, tags). To the <paramref name="enumerationCallback"/>
        /// </summary>
        /// <param name="batchSize">Batch size that single request to the Blockchain.Api can return</param>
        /// <param name="enumerationCallback">Enumeration callback, which will be called for every read asset</param>
        Task<EnumerationStatistics> EnumerateAllAssetsAsync(int batchSize, Func<BlockchainAsset, Task> enumerationCallback);

        /// <summary>
        /// Enumerates all blockchain assets (coins, tags). To the <paramref name="enumerationCallback"/>
        /// </summary>
        /// <param name="batchSize">Batch size that single request to the Blockchain.Api can return</param>
        /// <param name="enumerationCallback">Enumeration callback, which will be called for every read asset</param>
        Task<EnumerationStatistics> EnumerateAllAssetBatchesAsync(int batchSize, Action<IReadOnlyList<BlockchainAsset>> enumerationCallback);

        /// <summary>
        /// Enumerates all blockchain assets (coins, tags). To the <paramref name="enumerationCallback"/>
        /// </summary>
        /// <param name="batchSize">Batch size that single request to the Blockchain.Api can return</param>
        /// <param name="enumerationCallback">Enumeration callback, which will be called for every read asset</param>
        Task<EnumerationStatistics> EnumerateAllAssetBatchesAsync(int batchSize, Func<IReadOnlyList<BlockchainAsset>, Task> enumerationCallback);

        /// <summary>
        /// Returns all blockchain assets (coins, tags). If there are no assets, empty collection will be returned
        /// </summary>
        /// <param name="batchSize">Batch size that single request to the Blockchain.Api can return</param>
        Task<IReadOnlyDictionary<string, BlockchainAsset>> GetAllAssetsAsync(int batchSize);

        /// <summary>
        /// Should return specified asset (coin, tag)
        /// 
        /// Errors:
        /// - 204 No content: specified asset not found
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        Task<BlockchainAsset> GetAssetAsync(string assetId);

        /// <summary>
        /// Should return specified asset (coin, tag) or null if asset is not found
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        Task<BlockchainAsset> TryGetAssetAsync(string assetId);

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
        /// <see cref="EnumerateWalletBalanceBatchesAsync"/>, if the balance is non zero.
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
        /// </summary>
        /// <param name="batchSize">Maximum batch size</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        /// <param name="enumerationCallback">Batch enumeration callback</param>
        Task<EnumerationStatistics> EnumerateWalletBalanceBatchesAsync(int batchSize, Func<string, int> assetAccuracyProvider, Func<IReadOnlyList<WalletBalance>, Task<bool>> enumerationCallback);

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
        /// Tranaction amount is non acceptable.
        /// Transaction building should be retried with different amount
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
        /// Should return broadcasted  transaction by the operationId. All transactions, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here.
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        /// <returns>Broadcasted transaction or null</returns>
        Task<BroadcastedTransaction> TryGetBroadcastedTransactionAsync(Guid operationId, BlockchainAsset asset);

        /// <summary>
        /// Should return broadcasted transaction be the operationId. All transactions, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        /// <exception cref="ErrorResponseException">Status code: <see cref="HttpStatusCode.NoContent"/> - transaction is not found</exception>
        Task<BroadcastedTransaction> GetBroadcastedTransactionAsync(Guid operationId, BlockchainAsset asset);

        /// <summary> 
        /// Should remove specified transaction from the broadcasted transactions.
        /// Should affect transactions returned by the
        /// <see cref="GetBroadcastedTransactionAsync"/> and <see cref="TryGetBroadcastedTransactionAsync"/>
        /// </summary>
        Task<bool> ForgetBroadcastedTransactionsAsync(Guid operationId);

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
        /// Should affect result of the <see cref="GetHistoryOfIncomingTransactionsAsync"/>.
        /// </summary>
        /// <param name="address">Address for which incoming transactions history should be observed</param>
        /// <returns>
        /// true - if transactions observation is started. 
        /// false - if transactions observation was already started fot the given <paramref name="address"/>
        /// </returns>
        Task<bool> StartHistoryObservationOfIncomingTransactionsAsync(string address);

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
        Task<IEnumerable<HistoricalTransaction>> GetHistoryOfOutgoingTransactionsAsync(string address, string afterHash, int take, Func<string, int> assetAccuracyProvider);

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
        Task<IEnumerable<HistoricalTransaction>> GetHistoryOfIncomingTransactionsAsync(string address, string afterHash, int take, Func<string, int> assetAccuracyProvider);


        #endregion
    }
}
