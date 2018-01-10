using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <summary>
    /// While (re)building the transaction, blockchain response that transaction amount is non acceptable.
    /// Transaction (re)building should be retries with another amount.
    /// </summary>
    [PublicAPI]
    public class NonAcceptableAmountException : TransactionBuildingFailedException
    {
        public NonAcceptableAmountException(string message, ErrorResponseException innerException = null) :
            base(message, innerException)
        {
        }
    }
}
