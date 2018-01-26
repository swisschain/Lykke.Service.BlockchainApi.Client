using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Transaction input
    /// </summary>
    [PublicAPI]
    public class TransactionInput
    {
        /// <summary>
        /// Source address
        /// </summary>
        public string FromAddress { get; }

        /// <summary>
        /// Amount to transfer from the <see cref="FromAddress"/> or actual amount, 
        /// which is transferred from the <see cref="FromAddress"/>, depending on the context.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Transaction input
        /// </summary>
        public TransactionInput(string fromAddress, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(FromAddress))
            {
                throw new ArgumentException(nameof(fromAddress), "Source address is required");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount should be positive number");
            }

            FromAddress = fromAddress;
            Amount = amount;
        }

        /// <summary>
        /// Transaction input
        /// </summary>
        public TransactionInput(TransactionInputContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Transaction input not found");
            }
            if (string.IsNullOrWhiteSpace(contract.FromAddress))
            {
                throw new ResultValidationException("Source address is required", contract.FromAddress);
            }

            FromAddress = FromAddress;

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
        }

        /// <summary>
        /// Converts transaction input to the contract DTO
        /// </summary>
        public TransactionInputContract ToContract(int assetAccuracy)
        {
            return new TransactionInputContract
            {
                FromAddress = FromAddress,
                Amount = Conversions.CoinsToContract(Amount, assetAccuracy)
            };
        }
    }
}
