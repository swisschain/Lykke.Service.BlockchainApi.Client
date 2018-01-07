using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction building parameters
    /// Request body for the:
    /// - [POST] /api/transactions
    /// </summary>
    /// <remarks>
    /// Service should build not signed transaction. 
    /// If transaction with the specified operationId already was built,
    /// it should be ignored and regular response should be returned
    /// </remarks>
    [PublicAPI]
    public class BuildTransactionRequest : BaseTransactionBuildingRequest
    {
    }
}
