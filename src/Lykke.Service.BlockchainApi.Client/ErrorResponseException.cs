using System;
using System.Net;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <summary>
    /// Represents error response from the Blockchain API service
    /// </summary>
    [PublicAPI]
    public class ErrorResponseException : Exception
    {
        public ErrorResponse Error { get; }

        public HttpStatusCode StatusCode { get; }

        public ErrorResponseException(ErrorResponse error, ApiException inner) :
            base(error.GetSummaryMessage() ?? string.Empty, inner)
        {
            Error = error;
            StatusCode = inner.StatusCode;
        }
    }
}
