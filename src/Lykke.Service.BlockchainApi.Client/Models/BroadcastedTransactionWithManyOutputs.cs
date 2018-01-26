using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Broadcasted transaction with many outputs
    /// </summary>
    [PublicAPI]
    public class BroadcastedTransactionWithManyOutputs : BaseBroadcastedTransaction
    {
        /// <summary>
        /// Sources
        /// </summary>
        public IReadOnlyList<TransactionOutput> Outputs { get; }

        public BroadcastedTransactionWithManyOutputs(BroadcastedTransactionWithManyOutputsResponse contract, int assetAccuracy, Guid expectedOperationId) :
            base(contract, assetAccuracy, expectedOperationId)
        {
            if (contract.State == BroadcastedTransactionState.Completed && contract.Outputs == null)
            {
                throw new ResultValidationException("Outputs are required when transaction is completed");
            }

            Outputs = contract.State == BroadcastedTransactionState.Completed
                ? contract
                    .Outputs
                    .Select(o => new TransactionOutput(o, assetAccuracy))
                    .ToArray()
                : Array.Empty<TransactionOutput>();
        }
    }
}
