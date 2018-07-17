using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Addresses;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.BlockchainApi.Contract.Common;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    internal interface IBlockchainApi
    {
        #region General

        /// <summary>
        /// Should return some general service info. Used to check is service running
        /// </summary>
        [Get("/api/isalive")]
        Task<IsAliveResponse> GetIsAliveAsync();

        /// <summary>
        /// Should return API capabilities set. Each optional operation has corresponding flag in the capabilities. 
        /// Optional operations should be implemented if particular blockchain provides such functionality. 
        /// Any field in response of this endpoint can be empty, this should be treated as false value.
        /// </summary>
        [Get("/api/capabilities")]
        Task<CapabilitiesResponse> GetCapabilitiesAsync();

        /// <summary>
        /// Optional.
        /// 
        /// This endpoint should return blockchain integration constants if any are supported.
        /// If no constants are supported, this method can be not implemented.
        /// 
        /// Errors:
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// </summary>
        [Get("/api/constants")]
        Task<ConstantsResponse> GetConstantsAsync();

        #endregion


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
        /// <param name="address">
        /// Wallet address (for the blockchains with address mapping it must be underlying address)
        /// </param>
        [Get("/api/addresses/{address}/validity")]
        Task<AddressValidationResponse> IsAddressValidAsync(string address);

        /// <summary>
        /// Optional. See <see cref="GetCapabilitiesAsync"/>
        /// 
        /// Should return one or many blockchain explorer URLs for the given address.
        /// 
        /// Errors:
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// </summary>
        /// <param name="address">
        /// Wallet address (for the blockchains with address mapping it must be underlying address)
        /// </param>
        [Get("/api/addresses/{address}/explorer-url")]
        Task<string[]> GetAddressExplorerUrlsAsync(string address);

        /// <summary>
        /// Should return underlying (blockchain native) address for the given virtual address
        /// </summary>
        /// <param name="address">Virtual address</param>
        [Get("/api/addresses/{address}/underlying")]
        Task<UnderlyingAddressResponse> GetUnderlyingAddressAsync(string address);

        /// <summary>
        /// Should return virtual address for the given underlying (blockchain native) address
        /// </summary>
        /// <param name="address">Underlying address</param>
        [Get("/api/addresses/{address}/virtual")]
        Task<VirtualAddressResponse> GetVirtualAddressAsync(string address);

        #endregion


        #region Balances

        /// <summary>
        /// Should remember the wallet address to observe the wallet balance and return it in the 
        /// <see cref="GetWalletBalancesAsync"/>, if the balance is non zero.
        /// 
        /// If there was any balance on the wallet before this call, 
        /// it could be ignored at the discretion of the implementation 
        /// (not returned in the <see cref="GetWalletBalancesAsync"/>).
        ///
        /// Errors:
        /// - 409 Conflict: specified address is already observed.
        /// </summary>
        /// <param name="address">
        /// Wallet address (for the blockchains with address mapping it must be virtual address)
        /// </param>
        [Post("/api/balances/{address}/observation")]
        Task StartBalanceObservationAsync(string address);

        /// <summary>
        /// Should forget the wallet address and stop observe its balance.
        /// 
        /// Errors:
        /// - 204 No content: specified address is not observed
        /// </summary>
        /// <param name="address">
        /// Wallet address (for the blockchains with address mapping it must be virtual address)
        /// </param>
        [Delete("/api/balances/{address}/observation")]
        Task StopBalanceObservationAsync(string address);

        /// <summary>
        /// Should return balances of the observed wallets with non zero balances.
        /// Wallets balance observation is enabled by the 
        /// <see cref="StartBalanceObservationAsync"/> and disabled by the <see cref="StopBalanceObservationAsync"/>.
        /// If there are no balances to return, empty array should be returned.
        /// Amount of the returned balances should not exceed <paramref name="take"/>. 
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
        /// Should build not signed transaction to transfer from the single source to the single destination.
        /// If the transaction with the specified operationId has already been built by one of the [POST] /api/transactions/* call,
        /// it should be ignored and regular response (as in the first request) should be returned. For the blockchains where “send” and “receive” 
        /// transactions are distinguished, this endpoint builds “send” transactions.
        /// 
        /// Errors:
        /// - 400 BadRequest: With one of <see cref="BlockchainErrorCode"/> as <see cref="BlockchainErrorResponse.ErrorCode"/>.
        /// </summary>
        [Post("/api/transactions/single")]
        Task<BuildTransactionResponse> BuildSingleTransactionAsync([Body] BuildSingleTransactionRequest body);

        /// <summary>
        /// Optional. See <see cref="GetCapabilitiesAsync"/>
        /// Should build not signed “receive” transaction to receive funds previously sent 
        /// from the single source to the single destination. If the receive transaction with 
        /// the specified operationId has already been built by the [POST] /api/transactions/single/receive call,
        ///  it should be ignored and regular response (as in the first request) should be returned. 
        /// This endpoint should be implemented by the blockchains, which distinguishes “send” and “receive” 
        /// transactions and “receive” transaction requires the same private key as the “send”.
        /// 
        /// Errors:
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// </summary>
        [Post("/api/transactions/single/receive")]
        Task<BuildTransactionResponse> BuildSingleReceiveTransactionAsync([Body] BuildSingleReceiveTransactionRequest body);

        /// <summary>
        /// Optional. See <see cref="GetCapabilitiesAsync"/>
        /// 
        /// Should build not signed transaction with many inputs. If the transaction with the specified operationId has 
        /// already been built by one of the[POST] /api/transactions call, it should be ignored and regular response 
        /// (as in the first request) should be returned. Fee should be included in the specified amount.
        /// 
        /// Errors:
        /// - 400 BadRequest: With one of <see cref="BlockchainErrorCode"/> as <see cref="BlockchainErrorResponse.ErrorCode"/>.
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// </summary>
        [Post("/api/transactions/many-inputs")]
        Task<BuildTransactionResponse> BuildTransactionWithManyInputsAsync([Body] BuildTransactionWithManyInputsRequest body);

        /// <summary>
        /// Optional. See <see cref="GetCapabilitiesAsync"/>
        /// 
        /// Should build not signed transaction with many outputs. If the transaction with the specified operationId has 
        /// already been built by one of the[POST] /api/transactions call, it should be ignored and regular response 
        /// (as in the first request) should be returned. Fee should be added to the specified amount.
        /// 
        /// Errors:
        /// - 400 BadRequest: With one of <see cref="BlockchainErrorCode"/> as <see cref="BlockchainErrorResponse.ErrorCode"/>.
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// </summary>
        [Post("/api/transactions/many-outputs")]
        Task<BuildTransactionResponse> BuildTransactionWithManyOutputsAsync([Body] BuildTransactionWithManyOutputsRequest body);

        /// <summary>
        /// Optional. See <see cref="GetCapabilitiesAsync"/>
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
        /// - 400 BadRequest: With one of <see cref="BlockchainErrorCode"/> as <see cref="BlockchainErrorResponse.ErrorCode"/>.
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// </summary>
        [Put("/api/transactions")]
        Task<RebuildTransactionResponse> RebuildTransactionAsync([Body] RebuildTransactionRequest body);

        /// <summary>
        /// Should broadcast the signed transaction.
        /// 
        /// Errors:
        /// - 400 BadRequest: With one of <see cref="BlockchainErrorCode"/> as <see cref="BlockchainErrorResponse.ErrorCode"/>.
        /// - 409 Conflict: transaction with specified operationId and signedTransaction is already broadcasted.
        /// </summary>
        [Post("/api/transactions/broadcast")]
        Task BroadcastTransactionAsync([Body] BroadcastTransactionRequest body);

        /// <summary>
        /// Should return broadcasted  transaction by the operationId. All transactions with single input and output, 
        /// that were broadcasted by the  <see cref="BroadcastTransactionAsync"/> should be available here.
        /// 
        /// Errors:
        /// - 204 No content - specified transaction not found
        /// </summary>
        [Get("/api/transactions/broadcast/single/{operationId}")]
        Task<BroadcastedSingleTransactionResponse> GetBroadcastedSingleTransactionAsync(Guid operationId);

        /// <summary>
        /// Optional. See <see cref="GetCapabilitiesAsync"/>
        /// Should return broadcasted transaction by the operationId. All transactions with many inputs, that were broadcasted by the 
        /// <see cref="BroadcastTransactionAsync"/> should be available here.
        /// 
        /// Errors:
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// - 204 No content - specified transaction not found
        /// </summary>
        [Get("/api/transactions/broadcast/many-inputs/{operationId}")]
        Task<BroadcastedTransactionWithManyInputsResponse> GetBroadcastedTransactionWithManyInputsAsync(Guid operationId);

        /// <summary>
        /// Optional. See <see cref="GetCapabilitiesAsync"/>
        /// Should return broadcasted transaction by the operationId. All transactions with many outputs, that were broadcasted by the 
        /// <see cref="BroadcastTransactionAsync"/> should be available here.
        /// 
        /// Errors:
        /// - 501 Not Implemented - function is not implemented in the blockchain.
        /// - 204 No content - specified transaction not found
        /// </summary>
        [Get("/api/transactions/broadcast/many-outputs/{operationId}")]
        Task<BroadcastedTransactionWithManyOutputsResponse> GetBroadcastedTransactionWithManyOutputsAsync(Guid operationId);

        /// <summary>
        /// Should remove specified transaction from the broadcasted transactions. Should affect 
        /// transactions returned by the <see cref="GetBroadcastedSingleTransactionAsync"/>
        /// </summary>
        [Delete("/api/transactions/broadcast/{operationId}")]
        Task ForgetBroadcastedTransactionAsync(Guid operationId);

        #endregion


        #region Transactions history

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
        Task StartHistoryObservationOfIncomingTransactionsAsync(string address);

        /// <summary>
        /// Should return completed transactions that transfer fund from the <paramref name="address"/> and that 
        /// were broadcasted after the transaction with the hash equal to the <paramref name="afterHash"/>.
        /// Should include transactions broadcasted not using this API.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// </summary>
        /// <param name="address">
        /// Address for which outgoing transactions history should be returned.
        /// For the blockchains with address mapping, it could be virtual or underlying address.
        /// </param>
        /// <param name="afterHash">Hash of the transaction after which history should be returned</param>
        /// <param name="take">Maximum transactions to return</param>
        [Get("/api/transactions/history/from/{address}")]
        Task<IReadOnlyList<HistoricalTransactionContract>> GetHistoryOfOutgoingTransactionsAsync(string address, string afterHash, int take);

        /// <summary>
        /// Should return completed transactions that transfer fund to the <paramref name="address"/> and that 
        /// were broadcasted after the transaction with the hash equal to the <paramref name="afterHash"/>.
        /// Should include transactions broadcasted not using this API.
        /// If there are no transactions to return, empty array should be returned.
        /// Amount of the returned transactions should not exceed <paramref name="take"/>.
        /// </summary>
        /// <param name="address">
        /// Address for which incoming transactions history should be returned.
        /// For the blockchains with address mapping, it could be virtual or underlying address
        /// </param>
        /// <param name="afterHash">Hash of the transaction after which history should be returned</param>
        /// <param name="take">Maximum transactions to return</param>
        [Get("/api/transactions/history/to/{address}")]
        Task<IReadOnlyList<HistoricalTransactionContract>> GetHistoryOfIncomingTransactionsAsync(string address, string afterHash, int take);

        /// <summary>
        /// Should stop observation of the transactions that transfer fund from the address.
        /// Should affect result of the <see cref="GetHistoryOfOutgoingTransactionsAsync"/>}.
        /// 
        /// Errors:
        /// - 204 No content: transactions from the address are not observed.
        /// </summary>
        [Delete("/api/transactions/history/from/{address}/observation")]
        Task<bool> StopHistoryObservationOfOutgoingTransactionsAsync(string address);

        /// <summary>
        /// Should stop observation of the transactions that transfer fund to the address.
        /// Should affect result of the <see cref="GetHistoryOfIncomingTransactionsAsync"/>}.
        /// 
        /// Errors:
        /// - 204 No content: transactions from the address are not observed.
        /// </summary>
        [Delete("/api/transactions/history/to/{address}/observation")]
        Task<bool> StopHistoryObservationOfIncomingTransactionsAsync(string address);

        #endregion 
    }
}
