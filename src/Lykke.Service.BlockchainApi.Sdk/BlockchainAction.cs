using System;

namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Describes either income to or outcome from address/account.
    /// May represent either an input or an output for UTXO-based blockchain, 
    /// or one side of transfer for JSON-based blockchain.
    /// </summary>
    public class BlockchainAction
    {
        public BlockchainAction() {}
        public BlockchainAction(string actionId, long blockNumber, DateTime blockTime, string transactionHash, string address, string assetId, decimal amount) =>
            (ActionId, BlockNumber, BlockTime, TransactionHash, Address, AssetId, Amount) = 
            (actionId, blockNumber, blockTime, transactionHash, address, assetId, amount);

        /// <summary>
        /// Value which identifies the action within transaction.
        /// For UTXO-based blockchains it may be something like "vin_N" for input and "vout_N" for output.
        /// For JSON-based blockchains it should be an action sequence number or digest.
        /// </summary>
        public string   ActionId        { get; set; }
        public long     BlockNumber     { get; set; }
        public DateTime BlockTime       { get; set; }
        public string   TransactionHash { get; set; }
        public string   Address         { get; set; }
        public string   AssetId         { get; set; }
        /// <summary>
        /// Amount of change. Must be positive for income and negative for outcome.
        /// </summary>
        public decimal  Amount          { get; set; }
    }
}