using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Broadcasted transaction output
    /// </summary>
    [PublicAPI]
    public class BroadcastedTransactionOutput
    {
        /// <summary>
        /// Destination address
        /// 
        /// For the blockchains with address mapping,
        /// this could be virtual or underlying address.
        /// </summary>
        public string ToAddress { get; }

        /// <summary>
        /// Amount to transfer from the <see cref="ToAddress"/> or actual amount, 
        /// which is transferred from the <see cref="ToAddress"/>, depending on the context.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Broadcasted transaction output
        /// </summary>
        // ReSharper disable once SuggestBaseTypeForParameter
        public BroadcastedTransactionOutput(BroadcastedTransactionOutputContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Transaction input not found");
            }
            if (string.IsNullOrWhiteSpace(contract.ToAddress))
            {
                throw new ResultValidationException("Destination address is required", contract.ToAddress);
            }

            ToAddress = contract.ToAddress;

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
