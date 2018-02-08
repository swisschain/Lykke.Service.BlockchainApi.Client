using System.Diagnostics.CodeAnalysis;
using Lykke.Service.BlockchainApi.Contract;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    internal static class TransactionBroadcastingResultMapper
    {
        [SuppressMessage("ReSharper", "RedundantCaseLabel")]
        public static TransactionBroadcastingResult FromErrorCode(BlockchainErrorCode errorCode)
        {
            switch (errorCode)
            {
                case BlockchainErrorCode.AmountIsTooSmall:
                    return TransactionBroadcastingResult.AmountIsTooSmall;
                    
                case BlockchainErrorCode.NotEnoughtBalance:
                    return TransactionBroadcastingResult.NotEnoughBalance;

                case BlockchainErrorCode.Unknown:
                default:
                    throw new ResultValidationException("Invalid error code", errorCode);
            }
        }
    }
}
