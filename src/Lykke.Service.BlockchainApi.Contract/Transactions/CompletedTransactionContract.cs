using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Observed transaction, that is completed.
    /// Used in the:
    /// - [GET] /api/transactions/completed?take=integer&amp;skip=integer response
    /// </summary>
    [PublicAPI]
    public class CompletedTransactionContract : BaseHealthyObservedTransactionContract
    {
    }
}
