using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Common;

namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Blockchain interaction API
    /// </summary>
    public interface IBlockchainApi
    {
        /// <summary>
        /// Returns capabilities of current integration.
        /// </summary>
        /// <returns></returns>
        CapabilitiesResponse GetCapabilities();

        /// <summary>
        /// Returns predefined constants of current integration.
        /// </summary>
        /// <returns></returns>
        ConstantsResponse GetConstants();
        
        /// <summary>
        /// Returns URL to explore specified address.
        /// </summary>
        /// <param name="address">Blockchain address (account)</param>
        /// <returns>URL</returns>
        string[] GetExplorerUrl(string address);

        /// <summary>
        /// Returns true if specified address is valid blockchain address (account).
        /// </summary>
        /// <param name="address">Blockchain address (account)</param>
        /// <returns></returns>
        bool AddressIsValid(string address);

        /// <summary>
        /// Returns true if specified address (account) exists in blockchain.
        /// Should return <see cref="AddressIsValid"/> result if pre-creation is not actual and any valid address is "exist".
        /// </summary>
        /// <param name="address">Blockchain address (account)</param>
        /// <returns></returns>
        Task<bool> AddressIsExistAsync(string address);
        
        /// <summary>
        /// Returns last confirmed (irreversible) block number, taking in account blockchain specific.
        /// </summary>
        /// <returns></returns>
        Task<long> GetLastConfirmedBlockNumberAsync();

        /// <summary>
        /// Returns true if accounts balances can be efficiently retrieved from blockchain (by chunks, not one-by-one).
        /// Usually it's actual for UTXO-based blockchains.
        /// </summary>
        bool CanGetBalances { get; }

        /// <summary>
        /// Retrieves balances of specified addresses from blockchain.
        /// Only called if <see cref="CanGetBalances"/> is true.
        /// </summary>
        /// <param name="addresses">List of addresses to retrieve balances.</param>
        /// <param name="getAsset">Asset getter. Is useful for converting amount to/from base units.</param>
        /// <returns></returns>
        Task<BlockchainBalance[]> GetBalancesAsync(string[] addresses, Func<string, Task<IAsset>> getAsset);

        /// <summary>
        /// Builds transaction.
        /// Can throw <see cref="ArgumentException"/> if specified arguments are invalid.
        /// Can throw <see cref="BlockchainException"/> for well-known errors (if current blockchain state doesn't allow to build valid transaction).
        /// </summary>
        /// <param name="operationId">Operation (transaction) identifier.</param>
        /// <param name="asset">Operation asset.</param>
        /// <param name="actions">Transfers.</param>
        /// <param name="includeFee">true if fee should be included in transfer amount.</param>
        /// <returns></returns>
        Task<(string transactionContext, decimal fee, long expiration)> BuildTransactionAsync(Guid operationId, IAsset asset, IReadOnlyList<IOperationAction> actions, bool includeFee);

        /// <summary>
        /// Sends signed transaction to blockchain.
        /// IMPORTANT: should not throw if transaction affects blockchain in any manner!
        /// If transactin is sent, but there is any note, or remark, or sending result description, then it should be returned in result string.
        /// Can throw <see cref="ArgumentException"/> if specified transaction is invalid, and it is checked locally.
        /// Can throw <see cref="BlockchainException"/> for well-known errors.
        /// </summary>
        /// <param name="signedTransaction">Result of <see cref"IBlockchainSignService.SignTransactionAsync()"/> call.</param>
        /// <returns></returns>
        Task<string> BroadcastTransactionAsync(string signedTransaction);

        /// <summary>
        /// Retrieves current transaction state from blockchain.
        /// If state of transaction couldn't be determined exactly then InProgress state should be returned.
        /// If transaction is definitely expired then Failed state with BuildingShouldBeRepeated error code should be returned.
        /// </summary>
        /// <param name="transactionHash">Hash of transaction.</param>
        /// <param name="expiration">Expiration feature, determined on Build step. Is useful for recognizing expired transactions.</param>
        /// <param name="asset">Transaction asset. Is useful for converting amount to/from base units.</param>
        /// <returns></returns>
        Task<BlockchainTransaction> GetTransactionAsync(string transactionHash, long expiration, IAsset asset);

        /// <summary>
        /// Pushes specified address to blockchain node, if used node must "know" the address to maintain its balance.
        /// May be required for UTXO-based blockchains.
        /// </summary>
        /// <param name="address">Blockchain address.</param>
        /// <returns></returns>
        Task ObserveAddressAsync(string address);

        /// <summary>
        /// Removes specified address from blockchain node, if used node must "know" the address to maintain its balance.
        /// May be required for UTXO-based blockchains.
        /// </summary>
        /// <param name="address">Blockchain address.</param>
        /// <returns></returns>
        Task DeleteAddressObservationAsync(string address);

        /// <summary>
        /// Performs direct transfer for testing purposes.
        /// </summary>
        /// <param name="fromAddress">Withdrawal address.</param>
        /// <param name="fromPrivateKey">Withdrawal address private key.</param>
        /// <param name="toAddress">Deposit address.</param>
        /// <param name="asset">Asset to transfer.</param>
        /// <param name="amount">Amount to transfer.</param>
        /// <returns>Any meaniningful serializable result.</returns>
        Task<object> TestingTransfer(string fromAddress, string fromPrivateKey, string toAddress, IAsset asset, decimal amount);
    }
}