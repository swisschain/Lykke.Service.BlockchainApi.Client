using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction building result.
    /// Response for the:
    /// - [POST] /api/transactions
    /// </summary>
    [PublicAPI]
    public class BuildTransactionResponse : BaseTransactionBuildingResponse
    {
    }
}
