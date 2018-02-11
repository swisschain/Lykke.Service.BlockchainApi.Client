using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Broadcasted transaction with many inputs
    /// </summary>
    [PublicAPI]
    public class BroadcastedTransactionWithManyInputs : BaseBroadcastedTransaction
    {
        /// <summary>
        /// Sources
        /// </summary>
        public IReadOnlyList<BroadcastedTransactionInput> Inputs { get; }

        public BroadcastedTransactionWithManyInputs(BroadcastedTransactionWithManyInputsResponse contract, int assetAccuracy, Guid expectedOperationId) :
            base(contract, assetAccuracy, expectedOperationId)
        {
            if (contract.State == BroadcastedTransactionState.Completed && contract.Inputs == null)
            {
                throw new ResultValidationException("Inputs are required when transaction is completed");
            }

            Inputs = contract.State == BroadcastedTransactionState.Completed
                ? contract
                    .Inputs
                    .Select(i => new BroadcastedTransactionInput(i, assetAccuracy))
                    .ToArray()
                : Array.Empty<BroadcastedTransactionInput>();
        }
    }
}
