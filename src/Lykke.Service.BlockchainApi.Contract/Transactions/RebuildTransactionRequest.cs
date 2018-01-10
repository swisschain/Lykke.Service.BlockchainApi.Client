using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Transaction rebuilding parameters
    /// Request body for the:
    /// - [PUT] /api/transactions
    ///     Errors:
    ///         - 406 Not Acceptable: transaction can’t be built due to non acceptable amount (too small for example).
    /// </summary>
    /// <remarks>
    /// Service sould rebuild not signed transaction with the specified fee factor, 
    /// if applicable for the given blockchain. This should be implemented, 
    /// if blockchain allows transaction rebuilding (substitution) with new fee. 
    /// This will be called if transaction is stuck in the “in-progress” state for too long, 
    /// to try to execute transaction with higher fee.
    /// </remarks>
    [PublicAPI]
    public class RebuildTransactionRequest : BaseTransactionBuildingRequest
    {
        /// <summary>
        /// Multiplier for the transaction fee. 
        /// Blockchain should multiply regular fee
        /// by this factor.
        /// x = feeFactor * regularFee
        /// </summary>
        [JsonProperty("feeFactor")]
        public decimal FeeFactor { get; set; }
    }
}
