using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    internal static class TransactionBroadcastingResultMapper
    {
        public static TransactionBroadcastingResult FromContract(BroadcastTransactionResponse response)
        {
            if (response == null)
            {
                throw new ResultValidationException("Contract response is required");
            }

            switch (response.ErrorCode)
            {
                case null:
                    return TransactionBroadcastingResult.Success;

                case TransactionExecutionError.Unknown:
                    throw new ResultValidationException("Error code is not allowed", response.ErrorCode);

                case TransactionExecutionError.AmountIsTooSmall:
                    return TransactionBroadcastingResult.AmountIsTooSmall;
                    
                case TransactionExecutionError.NotEnoughtBalance:
                    return TransactionBroadcastingResult.NotEnoughBalance;

                default:
                    throw new ResultValidationException("Unknown error code", response.ErrorCode);
            }
        }
    }
}
