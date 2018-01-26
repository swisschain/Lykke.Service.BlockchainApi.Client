using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Transaction building result
    /// </summary>
    [PublicAPI]
    public class TransactionBuildingResult
    {
        /// <summary>
        /// Transaction execution error, if any that match one of the predefined code
        /// </summary>
        public TransactionExecutionError? Error { get; }

        /// <summary>
        /// The transaction context in the blockchain 
        /// specific format, which should be passed to the
        /// Blockchain.SignService for signing
        /// </summary>
        public string TransactionContext { get; }

        public TransactionBuildingResult(BaseTransactionBuildingResponse contract)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Contract value is required");
            }
            if (string.IsNullOrWhiteSpace(contract.TransactionContext) && !Error.HasValue)
            {
                throw new ResultValidationException("Either transaction context or error is required", contract.TransactionContext);
            }

            Error = contract.ErrorCode;
            TransactionContext = contract.TransactionContext;
        }
    }
}
