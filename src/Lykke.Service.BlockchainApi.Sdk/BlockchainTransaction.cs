using System;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Describes transaction state in blockchain
    /// </summary>
    public class BlockchainTransaction
    {
        public BlockchainTransaction() {}

        public static BlockchainTransaction Completed(long blockNumber, DateTime blockTime, BlockchainAction[] actions) =>
            new BlockchainTransaction { State = BroadcastedTransactionState.Completed, BlockNumber = blockNumber, BlockTime = blockTime, Actions = actions };

        public static BlockchainTransaction Failed(string error, BlockchainErrorCode errorCode) =>
            new BlockchainTransaction { State = BroadcastedTransactionState.Failed, Error = error, ErrorCode = errorCode };

        public static BlockchainTransaction InProgress() =>
            new BlockchainTransaction { State = BroadcastedTransactionState.InProgress };

        public BroadcastedTransactionState State       { get; set; }
        public long?                       BlockNumber { get; set; }
        public DateTime?                   BlockTime   { get; set; }
        public BlockchainAction[]          Actions     { get; set; }
        public string                      Error       { get; set; }
        public BlockchainErrorCode?        ErrorCode   { get; set; }
    }
}