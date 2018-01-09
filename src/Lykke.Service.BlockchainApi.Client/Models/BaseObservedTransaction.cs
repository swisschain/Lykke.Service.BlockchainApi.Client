using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Base observed transaction
    /// </summary>
    [PublicAPI]
    public abstract class BaseObservedTransaction
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// Transaction moment in UTC
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Source address
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination address
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// Asset ID
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Amount without fee
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Fee
        /// </summary>
        public decimal Fee { get; set; }

        protected BaseObservedTransaction(BaseObservedTransactionContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Contract value is required");
            }
            if (contract.OperationId == Guid.Empty)
            {
                throw new ResultValidationException("Operation ID is required", contract.OperationId);
            }
            if (contract.Timestamp == DateTime.MinValue)
            {
                throw new ResultValidationException("Timestamp is required", contract.Timestamp);
            }
            if (contract.Timestamp.Kind != DateTimeKind.Utc)
            {
                throw new ResultValidationException("Timestamp kind should be UTC", contract.Timestamp.Kind);
            }
            if (string.IsNullOrWhiteSpace(contract.FromAddress))
            {
                throw new ResultValidationException("Source address is required", contract.FromAddress);
            }
            if (string.IsNullOrWhiteSpace(contract.ToAddress))
            {
                throw new ResultValidationException("Destination address is required", contract.ToAddress);
            }
            if (string.IsNullOrWhiteSpace(contract.AssetId))
            {
                throw new ResultValidationException("Asset ID is required", contract.AssetId);
            }

            try
            {
                Amount = Conversions.CoinsFromContract(contract.Amount, assetAccuracy);
            }
            catch (ConversionException ex)
            {
                throw new ResultValidationException("Failed to parse amount", contract.Amount, ex);
            }

            try
            {
                Fee = Conversions.CoinsFromContract(contract.Fee, assetAccuracy);
            }
            catch (ConversionException ex)
            {
                throw new ResultValidationException("Failed to parse fee", contract.Fee, ex);
            }
        }
    }
}
