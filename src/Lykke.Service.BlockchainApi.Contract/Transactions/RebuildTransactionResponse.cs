using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction building result.
    /// Response for the:
    /// - [PUT] /api/transactions
    /// </summary>
    [PublicAPI]
    public class RebuildTransactionResponse : BaseTransactionBuildingResponse
    {
    }
}
