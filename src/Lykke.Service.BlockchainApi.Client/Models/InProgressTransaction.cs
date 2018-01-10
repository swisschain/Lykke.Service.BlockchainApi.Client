using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Observed transaction, being in the progress.
    /// </summary>
    [PublicAPI]
    public class InProgressTransaction : BaseObservedTransaction
    {
        /// <summary>
        /// Transaction hash as base64 string.
        /// Can be empty
        /// </summary>
        public string Hash { get; }

        public InProgressTransaction(InProgressTransactionContract contract, int assetAccuracy) :
            base(contract, assetAccuracy)
        {
            Hash = contract.Hash;
        }
    }
}
