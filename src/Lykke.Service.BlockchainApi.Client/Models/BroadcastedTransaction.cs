using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Broadcasted transaction
    /// </summary>
    [PublicAPI]
    public class BroadcastedTransaction 
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
        /// Amount without fee
        /// </summary>
        public decimal Amount { get; }
        
        /// <summary>
        /// Fee
        /// </summary>
        public decimal Fee { get; }

        /// <summary>
        /// Transaction hash as base64 string.
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Error description
        /// </summary>
        public string Error { get; }

        public BroadcastedTransaction(BroadcastedTransactionResponse contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Contract value is required");
            }
            if (contract.OperationId == Guid.Empty)
            {
                throw new ResultValidationException("Operation ID is required", contract.OperationId);
            }
            if (Enum.IsDefined(typeof(BroadcastedTransactionState), contract.State))
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

            try
            {
                Amount = Conversions.CoinsFromContract(contract.Amount, assetAccuracy);

                if (Amount <= 0)
                {
                    throw new ResultValidationException("Amount should be positive number", contract.Amount);
                }
            }
            catch (ConversionException ex)
            {
                throw new ResultValidationException("Failed to parse amount", contract.Amount, ex);
            }

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

            OperationId = contract.OperationId;
            State = contract.State;
            Timestamp = contract.Timestamp;
            Hash = contract.Hash;

            if(State == BroadcastedTransactionState.Failed)
            {
                Error = string.IsNullOrWhiteSpace(contract.Error)
                    ? "Blockchain API doesn't specify an error message"
                    : contract.Error;
            }
        }
    }
}
