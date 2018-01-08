using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Observed transaction, that is completed.
    /// </summary>
    [PublicAPI]
    public class CompletedTransaction : BaseHealthyObservedTransaction
    {
        public CompletedTransaction(CompletedTransactionContract contract, int assetAccuracy) : 
            base(contract, assetAccuracy)
        {
        }
    }
}
