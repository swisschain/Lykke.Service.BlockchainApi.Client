﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Client.Results;

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
        Task<IEnumerable<BlockchainAsset>> GetAssetsAsync();

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
        /// Wallets balance observation is enabled by the <see cref="StartBalanceObservationAsync"/> and 
        /// disabled by the <see cref="StopBalanceObservationAsync"/>.
        /// If there are no balances to return, empty array should be returned.
        /// Amount of the returned wallets should not exceed <paramref name="take"/>.
        /// <paramref name="skip"/> balances should be skipped before return first balance.
        /// </summary>
        /// <param name="take">Maximum wallets to return</param>
        /// <param name="skip">Amount of the wallets to skip before return first wallet</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<IEnumerable<WalletBalance>> GetWalletBalancesAsync(int take, int skip, Func<string, int> assetAccuracyProvider);

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
        Task<TransactionBuildingResult> BuildTransactionAsync(Guid operationId, string fromAddress, string toAddress, BlockchainAsset asset, decimal amount, bool includeFee);

        /// <summary>
        /// Optional method.
        /// 
        /// Should rebuild not signed transaction with the specified fee factor, 
        /// if applicable for the given blockchain. This should be implemented, 
        /// if blockchain allows transaction rebuilding (substitution) with new fee. 
        /// This will be called if transaction is stuck in the “in-progress” state for too long,
        /// to try to execute transaction with higher fee.
        /// </summary>
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="fromAddress">Source address</param>
        /// <param name="toAddress">Destination address</param>
        /// <param name="asset">Blockchain asset to transfer</param>
        /// <param name="amount">Amount to transfer</param>
        /// <param name="includeFee">Flag, which indicates, that fee should be included in the specified amount</param>
        /// <param name="feeFactor">Multiplier for the transaction fee. Blockchain will multiply regular fee by this factor</param>
        Task<TransactionBuildingResult> RebuildTransactionAsync(Guid operationId, string fromAddress, string toAddress, BlockchainAsset asset, decimal amount, bool includeFee, decimal feeFactor);

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
        /// Transaction should be removed from this collection when its state is changed to the completed or failed
        /// <paramref name="skip"/> transactions should be skipped before return first transaction.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="skip">Amount of the transactions to skip before return first transaction</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<IEnumerable<InProgressTransaction>> GetInProgressTransactionsAsync(int take, int skip, Func<string, int> assetAccuracyProvider);

        /// <summary>
        /// Should return completed observed transactions. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// <paramref name="skip"/> transactions should be skipped before return first transaction.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="skip">Amount of the transactions to skip before return first transaction</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<IEnumerable<CompletedTransaction>> GetCompletedTransactionsAsync(int take, int skip, Func<string, int> assetAccuracyProvider);

        /// <summary>
        /// Should return failed observed transactions. Transaction observation is started when 
        /// transaction is broadcasted by <see cref="BroadcastTransactionAsync"/>.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// <paramref name="skip"/> transactions should be skipped before return first transaction.
        /// </summary>
        /// <param name="take">Maximum transactions to return</param>
        /// <param name="skip">Amount of the transactions to skip before return first transaction</param>
        /// <param name="assetAccuracyProvider">Delegate which should provide blockchain asset pair accuracy by the blockchain asset ID</param>
        Task<IEnumerable<FailedTransaction>> GetFailedTransactionsAsync(int take, int skip, Func<string, int> assetAccuracyProvider);

        /// <summary>
        /// Should stop observation of the specified transactions. 
        /// If one or many of the specified transactions not found in the observed transactions, 
        /// they should be ignored. Should affect transactions list returned by the
        /// <see cref="GetCompletedTransactionsAsync"/> and <see cref="GetFailedTransactionsAsync"/>
        /// </summary>
        Task StopTransactionsObservationAsync(IReadOnlyList<Guid> operationIds);

        #endregion
    }
}
