using System;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <summary>
    /// General exception for transaction building errors
    /// </summary>
    [PublicAPI]
    public class TransactionBuildingFailedException : Exception
    {
        public TransactionBuildingFailedException(string message, ErrorResponseException innerException = null) :
            base(message, innerException)
        {
        }
    }
}
