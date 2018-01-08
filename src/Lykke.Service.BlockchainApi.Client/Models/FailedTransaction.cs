using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Observed transaction, that is failed.
    /// </summary>
    [PublicAPI]
    public class FailedTransaction : BaseObservedTransaction
    {
        /// <summary>
        /// Error description
        /// </summary>
        public string Error { get; set; }

        public FailedTransaction(FailedTransactionContract contract, int assetAccuracy) :
            base(contract, assetAccuracy)
        {
            Error = string.IsNullOrWhiteSpace(contract.Error)
                ? "Blockchain API doesn't specify an error message"
                : contract.Error;
        }
    }
}
