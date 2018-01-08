using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Base healthy (not failed) observed transaction
    /// </summary>
    [PublicAPI]
    public abstract class BaseHealthyObservedTransaction : BaseObservedTransaction
    {
        /// <summary>
        /// Transaction hash as base64 string.
        /// Can be empty
        /// </summary>
        public string Hash { get; set; }

        protected BaseHealthyObservedTransaction(BaseHealthyObservedTransactionContract contract, int assetAccuracy) :
            base(contract, assetAccuracy)
        {
            Hash = contract.Hash;
        }
    }
}
