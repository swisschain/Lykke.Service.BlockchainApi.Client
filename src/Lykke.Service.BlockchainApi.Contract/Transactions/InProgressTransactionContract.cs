using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Observed transaction, being in the progress.
    /// Used in the:
    /// - [GET] /api/transactions/in-progress?take=integer&amp;skip=integer response
    /// </summary>
    [PublicAPI]
    public class InProgressTransactionContract : BaseHealthyObservedTransactionContract
    {
    }
}
