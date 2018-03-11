using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Broadcasted single input and output transaction
    /// </summary>
    [PublicAPI]
    public class BroadcastedSingleTransaction : BaseBroadcastedTransaction
    {
        /// <summary>
        /// Amount without fee
        /// Should be positive number if the <see cref="BaseBroadcastedTransaction.State"/> is <see cref="BroadcastedTransactionState.Completed"/>
        /// </summary>
        public decimal Amount { get; }
        
        public BroadcastedSingleTransaction(BroadcastedSingleTransactionResponse contract, int assetAccuracy, Guid expectedOperationId) :
            base(contract, assetAccuracy, expectedOperationId)
        {
            if (!string.IsNullOrEmpty(contract.Amount))
            {
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
}
