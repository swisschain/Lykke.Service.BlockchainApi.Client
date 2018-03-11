using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Base broadcasted transaction
    /// </summary>
    [PublicAPI]
    public abstract class BaseBroadcastedTransaction
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        public Guid OperationId { get; }

        /// <summary>
        /// State
        /// </summary>
        public BroadcastedTransactionState State { get; }

        /// <summary>
        /// Transaction moment in UTC
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Fee
        /// Should be positive number if the <see cref="State"/> is <see cref="BroadcastedTransactionState.Completed"/>
        /// </summary>
        public decimal Fee { get; }

        /// <summary>
        /// Transaction hash as base64 string.
        /// Should be non empty if the <see cref="State"/> is <see cref="BroadcastedTransactionState.Completed"/>
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Error description
        /// Should be non empty if the <see cref="State"/> is <see cref="BroadcastedTransactionState.Failed"/>
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Incremental ID of the moment, when the transaction
        /// state changing is detected. It should be the same
        /// sequence as for <see cref="WalletBalance.Block"/>. 
        /// For the most blockchains it could be the block number/height.
        /// </summary>
        public long Block { get; }

        protected BaseBroadcastedTransaction(BaseBroadcastedTransactionResponse contract, int assetAccuracy, Guid expectedOperationId)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Transaction not found");
            }
            if (contract.OperationId == Guid.Empty)
            {
                throw new ResultValidationException("Operation ID is required", contract.OperationId);
            }
            if (contract.OperationId != expectedOperationId)
            {
                throw new ResultValidationException($"Unexpected operation ID {contract.OperationId} in the transaction. Expected operation ID: {expectedOperationId}");
            }
            if (!Enum.IsDefined(typeof(BroadcastedTransactionState), contract.State))
            {
                throw new ResultValidationException("Unknown transaction state", contract.State);
            }
            if (contract.Timestamp == DateTime.MinValue)
            {
                throw new ResultValidationException("Timestamp is required", contract.Timestamp);
            }
            if (contract.Timestamp.Kind != DateTimeKind.Utc)
            {
                throw new ResultValidationException("Timestamp kind should be UTC", contract.Timestamp.Kind);
            }
            if (contract.State == BroadcastedTransactionState.Completed && string.IsNullOrWhiteSpace(contract.Hash))
            {
                throw new ResultValidationException("Hash is required for the completed transaction", contract.Hash);
            }
            if (contract.Block == 0)
            {
                throw new ResultValidationException("Block is required", contract.Block);
            }

            if (!string.IsNullOrEmpty(contract.Fee))
            {
                try
                {
                    Fee = Conversions.CoinsFromContract(contract.Fee, assetAccuracy);

                    if (Fee < 0)
                    {
                        throw new ResultValidationException("Fee can't be negative number", contract.Fee);
                    }
                }
                catch (ConversionException ex)
                {
                    throw new ResultValidationException("Failed to parse fee", contract.Fee, ex);
                }
            }

            OperationId = contract.OperationId;
            State = contract.State;
            Timestamp = contract.Timestamp;
            Hash = contract.Hash;
            Block = contract.Block;

            if (State == BroadcastedTransactionState.Failed)
            {
                Error = string.IsNullOrWhiteSpace(contract.Error)
                    ? "Blockchain API doesn't specify an error message"
                    : contract.Error;
            }
        }
    }
}
