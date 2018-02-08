using System;
using System.Net;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <summary>
    /// Represents error response from the Blockchain API service
    /// </summary>
    [PublicAPI]
    public class ErrorResponseException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public BlockchainErrorCode ErrorCode => Error.ErrorCode;

        /// <summary>
        /// Erorr response
        /// </summary>
        public BlockchainErrorResponse Error { get; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        public ErrorResponseException(BlockchainErrorResponse error, ApiException inner) :
            base(error.GetSummaryMessage(), inner)
        {
            Error = error;
            StatusCode = inner.StatusCode;
        }
    }
}
