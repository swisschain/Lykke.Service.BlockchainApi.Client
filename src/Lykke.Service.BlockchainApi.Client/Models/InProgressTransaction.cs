using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Observed transaction, being in the progress.
    /// </summary>
    [PublicAPI]
    public class InProgressTransaction : BaseHealthyObservedTransaction
    {
        public InProgressTransaction(InProgressTransactionContract contract, int assetAccuracy) : 
            base(contract, assetAccuracy)
        {
        }
    }
}
