using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Broadcasted transaction input
    /// </summary>
    [PublicAPI]
    public class BroadcastedTransactionInput
    {
        /// <summary>
        /// Source address
        /// 
        /// For the blockchains with address mapping,
        /// this could be virtual address.
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
        // ReSharper disable once SuggestBaseTypeForParameter
        public BroadcastedTransactionInput(BroadcastedTransactionInputContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Transaction input not found");
            }
            if (string.IsNullOrWhiteSpace(contract.FromAddress))
            {
                throw new ResultValidationException("Source address is required", contract.FromAddress);
            }

            FromAddress = contract.FromAddress;

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
    }
}
