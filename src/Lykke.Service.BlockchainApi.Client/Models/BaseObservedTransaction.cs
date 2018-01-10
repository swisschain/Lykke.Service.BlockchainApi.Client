using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Base observed transaction
    /// </summary>
    [PublicAPI]
    public abstract class BaseObservedTransaction : BaseTransaction
    {
        /// <summary>
        /// Fee
        /// </summary>
        public decimal Fee { get; }

        protected BaseObservedTransaction(BaseObservedTransactionContract contract, int assetAccuracy) : 
            base(contract, assetAccuracy)
        {
            try
            {
                Fee = Conversions.CoinsFromContract(contract.Fee, assetAccuracy);
            }
            catch (ConversionException ex)
            {
                throw new ResultValidationException("Failed to parse fee", contract.Fee, ex);
            }
        }
    }
}
