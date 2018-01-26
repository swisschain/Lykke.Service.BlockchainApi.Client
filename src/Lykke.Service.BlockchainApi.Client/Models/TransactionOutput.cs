using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Transaction output
    /// </summary>
    [PublicAPI]
    public class TransactionOutput
    {
        /// <summary>
        /// Destination address
        /// </summary>
        public string ToAddress { get; }

        /// <summary>
        /// Amount to transfer from the <see cref="ToAddress"/> or actual amount, 
        /// which is transferred from the <see cref="ToAddress"/>, depending on the context.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Transaction output
        /// </summary>
        public TransactionOutput(string toAddress, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(ToAddress))
            {
                throw new ArgumentException(nameof(toAddress), "Destination address is required");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount should be positive number");
            }

            ToAddress = toAddress;
            Amount = amount;
        }

        /// <summary>
        /// Transaction output
        /// </summary>
        public TransactionOutput(TransactionOutputContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Transaction input not found");
            }
            if (string.IsNullOrWhiteSpace(contract.ToAddress))
            {
                throw new ResultValidationException("Destination address is required", contract.ToAddress);
            }

            ToAddress = ToAddress;

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
        /// Converts transaction output to the contract DTO
        /// </summary>
        /// <param name="assetAccuracy"></param>
        /// <returns></returns>
        public TransactionOutputContract ToContract(int assetAccuracy)
        {
            return new TransactionOutputContract
            {
                ToAddress = ToAddress,
                Amount = Conversions.CoinsToContract(Amount, assetAccuracy)
            };
        }
    }
}
