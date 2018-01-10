using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction building result.
    /// Response for the:
    /// - [POST] /api/transactions
    ///     Errors:
    ///         - 406 Not Acceptable: transaction can’t be built due to non acceptable amount (too small for example).
    /// </summary>
    [PublicAPI]
    public class BuildTransactionResponse : BaseTransactionBuildingResponse
    {
    }
}
