using System;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <summary>
    /// This exception could be thrown on transaction building, if transaction has been alread
    /// broadcasted or even removed
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class TransactionAlreadyBroadcastedException : Exception
    {
        /// <summary>
        /// This exception could be thrown on transaction building, if transaction has been alread
        /// broadcasted or even removed
        /// </summary>
        public TransactionAlreadyBroadcastedException(ErrorResponseException ex) :
            base("Transaction has been already broadcasted or removed", ex)
        {
        }
    }
}
