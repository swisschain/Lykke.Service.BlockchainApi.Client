using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    [PublicAPI]
    public class HistoricalTransaction
    {
        /// <summary>
        /// Transaction moment in UTC
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Source address
        /// 
        /// For the blockchains with address mapping,
        /// this should be underlying (real) address
        /// </summary>
        public string FromAddress { get; }

        /// <summary>
        /// Destination address
        /// 
        /// For the blockchains with address mapping,
        /// this should be underlying (real) address
        /// </summary>
        public string ToAddress { get; }

        /// <summary>
        /// Asset ID
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// Amount without fee
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Transaction hash as base64 string.
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Type of the transaction.
        /// Can be empty.
        /// Should be non empty if the flag
        /// <see cref="BlockchainCapabilities.IsReceiveTransactionRequired"/> is true
        /// </summary>
        [CanBeNull]
        public TransactionType? TransactionType { get; }

        public HistoricalTransaction(HistoricalTransactionContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Transaction not found");
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
            if (string.IsNullOrWhiteSpace(contract.Hash))
            {
                throw new ResultValidationException("Hash is required", contract.Hash);
            }
            if (contract.TransactionType.HasValue && !Enum.IsDefined(typeof(TransactionType), contract.TransactionType))
            {
                throw new ResultValidationException("Unknown transaction type", contract.TransactionType);
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

            Timestamp = contract.Timestamp;
            FromAddress = contract.FromAddress;
            ToAddress = contract.ToAddress;
            AssetId = contract.AssetId;
            Hash = contract.Hash;
            TransactionType = contract.TransactionType;
        }
    }
}
