using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Contract;

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

        /// <summary>
        /// Should return blockchain API capabilities.
        /// </summary>
        Task<BlockchainCapabilities> GetCapabilitiesAsync();

        /// <summary>
        /// Should return blockchain API constants.
        /// </summary>
        Task<BlockchainConstants> GetConstantsAsync();
        
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

        /// <summary>
        /// Optional method. <see cref="GetCapabilitiesAsync"/>.
        /// 
        /// Should return one or many blockchain explorer URLs for the given address.
        /// </summary>
        /// <param name="address">Wallet address</param>
        Task<IReadOnlyList<Uri>> GetAddressExplorerUrlAsync(string address);

        /// <summary>
        /// Should return underlying (blockchain native) address for the given virtual address
        /// </summary>
        /// <param name="virtualAddress">Virtual address</param>
        Task<string> GetAddressUnderlying(string virtualAddress);

        /// <summary>
        /// Should return virtual address for the given underlying (blockchain native) address
        /// </summary>
        /// <param name="underlyingAddress">Underlying address</param>
        Task<string> GetAddressVirtual(string underlyingAddress);

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


        #region Transactions building

        /// <summary>
        /// Should build not signed transaction to transfer from the single source to the single destination. If the transaction with the specified 
        /// <paramref name="operationId"/> has already been built by one of the
        /// <see cref="BuildSingleTransactionAsync"/>,
        /// <see cref="BuildTransactionWithManyInputsAsync"/> or
        /// <see cref="BuildTransactionWithManyOutputsAsync"/>, 
        /// call, it should be ignored and regular response (as in the first request) should be returned.
        /// For the blockchains where “send” and “receive” transactions are distinguished, this endpoint builds “send” transactions.
        /// </summary>
        /// 
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="fromAddress">Source address</param>
        /// <param name="fromAddressContext">Source address context taken from the blockchain sign service</param>
        /// <param name="toAddress">Destination address</param>
        /// <param name="asset">Blockchain asset to transfer</param>
        /// <param name="amount">Amount to transfer</param>
        /// <param name="includeFee">Flag, which indicates, that fee should be included in the specified amount</param>
        /// 
        /// <exception cref="ErrorResponseException">
        /// Among <see cref="BlockchainErrorCode.Unknown"/> error next error codes can be specified:
        /// - <see cref="BlockchainErrorCode.AmountIsTooSmall"/>
        /// - <see cref="BlockchainErrorCode.NotEnoughBalance"/>
        /// </exception>
        /// <exception cref="TransactionAlreadyBroadcastedException">
        /// Transaction has been already broadcasted or even removed
        /// </exception>
        Task<TransactionBuildingResult> BuildSingleTransactionAsync(Guid operationId, string fromAddress, string fromAddressContext, string toAddress, BlockchainAsset asset, decimal amount, bool includeFee);

        /// <summary>
        /// Optional method. <see cref="GetCapabilitiesAsync"/>
        ///  
        /// Should build not signed receive transaction to receive funds transfered by the send transaction from the single source to the single destination. 
        /// If transaction with the specified <paramref name="operationId"/> already was built by the
        /// <see cref="BuildSingleReceiveTransactionAsync"/> it should be ignored and regular response should be returned.
        /// </summary>
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="sendTransactionHash">Hash of the send transaction, which should be received</param>
        /// <exception cref="TransactionAlreadyBroadcastedException">
        /// Transaction has been already broadcasted or even removed
        /// </exception>
        Task<TransactionBuildingResult> BuildSingleReceiveTransactionAsync(Guid operationId, string sendTransactionHash);

        /// <summary>
        /// Optional method. <see cref="GetCapabilitiesAsync"/>
        /// 
        /// Should build not signed transaction to transfer from the single source to the single destination. If transaction with the specified 
        /// <paramref name="operationId"/> already was built by one of the
        /// <see cref="BuildSingleTransactionAsync"/>,
        /// <see cref="BuildTransactionWithManyInputsAsync"/> or
        /// <see cref="BuildTransactionWithManyOutputsAsync"/>, 
        /// it should be ignored and regular response should be returned.
        /// Fee should be included in the specified amount.
        /// </summary>
        /// 
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="inputs">Sources</param>
        /// <param name="toAddress">Destination address</param>
        /// <param name="asset">Blockchain asset to transfer</param>
        /// 
        /// <exception cref="ErrorResponseException">
        /// Among <see cref="BlockchainErrorCode.Unknown"/> error next error codes can be specified:
        /// - <see cref="BlockchainErrorCode.AmountIsTooSmall"/>
        /// - <see cref="BlockchainErrorCode.NotEnoughBalance"/>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Operation is not supported for the given blockchain. See <see cref="GetCapabilitiesAsync"/>
        /// </exception>
        /// <exception cref="TransactionAlreadyBroadcastedException">
        /// Transaction has been already broadcasted or even removed
        /// </exception>
        Task<TransactionBuildingResult> BuildTransactionWithManyInputsAsync(Guid operationId, IEnumerable<BuildingTransactionInput> inputs, string toAddress, BlockchainAsset asset);

        /// <summary>
        /// Optional method. <see cref="GetCapabilitiesAsync"/>
        /// Should build not signed transaction to transfer from the single source to the single destination. If transaction with the specified 
        /// <paramref name="operationId"/> already was built by one of the
        /// <see cref="BuildSingleTransactionAsync"/>,
        /// <see cref="BuildTransactionWithManyInputsAsync"/> or
        /// <see cref="BuildTransactionWithManyOutputsAsync"/>, 
        /// it should be ignored and regular response should be returned.
        /// Fee should be added to the specified amount.
        /// </summary>
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="fromAddress">Destination address</param>
        /// <param name="fromAddressContext">context taken from the blockchain sign service</param>
        /// <param name="outputs">Destinations</param>
        /// <param name="asset">Blockchain asset to transfer</param>
        /// <exception cref="ErrorResponseException">
        /// Among <see cref="BlockchainErrorCode.Unknown"/> error next error codes can be specified:
        /// - <see cref="BlockchainErrorCode.AmountIsTooSmall"/>
        /// - <see cref="BlockchainErrorCode.NotEnoughBalance"/>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Operation is not supported for the given blockchain. See <see cref="GetCapabilitiesAsync"/>
        /// </exception>
        /// <exception cref="TransactionAlreadyBroadcastedException">
        /// Transaction has been already broadcasted or even removed
        /// </exception>
        Task<TransactionBuildingResult> BuildTransactionWithManyOutputsAsync(Guid operationId, string fromAddress, string fromAddressContext, IEnumerable<BuildingTransactionOutput> outputs, BlockchainAsset asset);


        /// <summary>
        /// Optional method. <see cref="GetCapabilitiesAsync"/>
        /// 
        /// Should rebuild not signed transaction with the specified fee factor, 
        /// if applicable for the given blockchain. This should be implemented, 
        /// if blockchain allows transaction rebuilding (substitution) with new fee. 
        /// This will be called if transaction is stuck in the “in-progress” state for too long,
        /// to try to execute transaction with higher fee. <see cref="BuildSingleTransactionAsync"/> with 
        /// the same <paramref name="operationId"/> should precede to the given call. 
        /// Transaction should be rebuilt with parameters that were passed to the <see cref="BuildSingleTransactionAsync"/>.
        /// </summary>
        /// 
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="feeFactor">Multiplier for the transaction fee. Blockchain will multiply regular fee by this factor</param>
        /// 
        /// <exception cref="ErrorResponseException">
        /// Among <see cref="BlockchainErrorCode.Unknown"/> error next error codes can be specified:
        /// - <see cref="BlockchainErrorCode.AmountIsTooSmall"/>
        /// - <see cref="BlockchainErrorCode.NotEnoughBalance"/>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Operation is not supported for the given blockchain. See <see cref="GetCapabilitiesAsync"/>
        /// </exception>
        /// <exception cref="TransactionAlreadyBroadcastedException">
        /// Transaction has been already broadcasted or even removed
        /// </exception>
        Task<TransactionBuildingResult> RebuildTransactionAsync(Guid operationId, decimal feeFactor);

        #endregion


        #region Transactions broadcasting

        /// <summary>
        /// Should broadcast the signed transaction and start to observe its execution.
        /// 
        /// Errors:
        /// - 409 Conflict: transaction with specified operationId and signedTransaction is already broadcasted.
        /// </summary>
        /// <param name="operationId">Lykke unique operation ID</param>
        /// <param name="signedTransaction">The signed transaction returned by the Blockchain.SignService after signing</param>
        /// <returns>Transaction broadcasting result </returns>
        /// <exception cref="ErrorResponseException">
        /// Among <see cref="BlockchainErrorCode.Unknown"/> error next error codes can be specified:
        /// - <see cref="BlockchainErrorCode.AmountIsTooSmall"/>
        /// - <see cref="BlockchainErrorCode.NotEnoughBalance"/>
        /// - <see cref="BlockchainErrorCode.BuildingShouldBeRepeated"/>
        /// </exception>
        Task<TransactionBroadcastingResult> BroadcastTransactionAsync(Guid operationId, string signedTransaction);

        /// <summary>
        /// Should return broadcasted  transaction by the operationId. All transactions with single input and output, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here.
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        /// <returns>Broadcasted transaction or null</returns>
        Task<BroadcastedSingleTransaction> TryGetBroadcastedSingleTransactionAsync(Guid operationId, BlockchainAsset asset);

        /// <summary>
        /// Should return broadcasted transaction be the operationId. All transactions with single input and output, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        /// <exception cref="ErrorResponseException">Status code: <see cref="HttpStatusCode.NoContent"/> - transaction is not found</exception>
        Task<BroadcastedSingleTransaction> GetBroadcastedSingleTransactionAsync(Guid operationId, BlockchainAsset asset);

        /// <summary>
        /// Should return broadcasted transaction be the operationId. All transactions with many inputs, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        Task<BroadcastedTransactionWithManyInputs> TryGetBroadcastedTransactionWithManyInputsAsync(Guid operationId, BlockchainAsset asset);

        /// <summary>
        /// Should return broadcasted transaction be the operationId. All transactions with many outputs, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        /// <exception cref="ErrorResponseException">Status code: <see cref="HttpStatusCode.NoContent"/> - transaction is not found</exception>
        Task<BroadcastedTransactionWithManyInputs> GetBroadcastedTransactionWithManyInputsAsync(Guid operationId, BlockchainAsset asset);

        /// <summary>
        /// Should return broadcasted transaction be the operationId. All transactions with many inputs, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        Task<BroadcastedTransactionWithManyOutputs> TryGetBroadcastedTransactionWithManyOutputsAsync(Guid operationId, BlockchainAsset asset);

        /// <summary>
        /// Should return broadcasted transaction be the operationId. All transactions with many outputs, that were broadcasted 
        /// by the <see cref="BroadcastTransactionAsync"/> should be available here
        /// </summary>
        /// <param name="operationId">Operation ID</param>
        /// <param name="asset">Transaction asset for amount calculation</param>
        /// <exception cref="ErrorResponseException">Status code: <see cref="HttpStatusCode.NoContent"/> - transaction is not found</exception>
        Task<BroadcastedTransactionWithManyOutputs> GetBroadcastedTransactionWithManyOutputsAsync(Guid operationId, BlockchainAsset asset);

        /// <summary> 
        /// Should remove specified transaction from the broadcasted transactions.
        /// Should affect transactions returned by the
        /// <see cref="GetBroadcastedSingleTransactionAsync"/> and <see cref="TryGetBroadcastedSingleTransactionAsync"/>
        /// </summary>
        Task<bool> ForgetBroadcastedTransactionsAsync(Guid operationId);

        #endregion


        #region Transactions history

        /// <summary>
        /// Should start observation of the transactions that transfer fund from the address. 
        /// Should affect result of the <see cref="GetHistoryOfOutgoingTransactionsAsync"/>.
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


        /// <summary>
        /// Should stop observation of the transactions that transfer fund from the address. 
        /// Should affect result of the <see cref="GetHistoryOfOutgoingTransactionsAsync"/>.
        /// </summary>
        /// <param name="address">Address for which outgoing transactions history observation should be stopped</param>
        /// <returns>
        /// true - if transactions observation is stopped. 
        /// false - if transactions observation for the given <paramref name="address"/> was not started yet 
        /// </returns>
        Task<bool> StopHistoryObservationOfOutgoingTransactionsAsync(string address);

        /// <summary>
        /// Should stop observation of the transactions that transfer fund to the address. 
        /// Should affect result of the <see cref="GetHistoryOfIncomingTransactionsAsync"/>.
        /// </summary>
        /// <param name="address">Address for which incoming transactions history should be stopped</param>
        /// <returns>
        /// true - if transactions observation is sopped. 
        /// false - if transactions observation for the given <paramref name="address"/> was not started yet
        /// </returns>
        Task<bool> StopHistoryObservationOfIncomingTransactionsAsync(string address);

        #endregion
    }
}
