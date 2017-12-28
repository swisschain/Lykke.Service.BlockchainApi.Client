using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;

namespace Lykke.Service.BlockchainApi.Client
{
    /// <summary>
    /// Represents error response from the Blockchain API service
    /// </summary>
    [PublicAPI]
    public class ErrorResponseException : Exception
    {
        public ErrorResponse Error { get; }

        public ErrorResponseException(ErrorResponse error) :
            base(BuildMessage(error))
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        public ErrorResponseException(ErrorResponse error, Exception inner) :
            base(BuildMessage(error), inner)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        private static string BuildMessage(ErrorResponse errorResponse)
        {
            if (errorResponse == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            if (errorResponse.ErrorMessage != null)
            {
                sb.AppendLine($"Error summary: {errorResponse.ErrorMessage}");
            }

            if (errorResponse.ModelErrors != null)
            {
                sb.AppendLine();

                foreach (var error in errorResponse.ModelErrors)
                {
                    if (error.Key != null && error.Value != null)
                    {
                        if (!string.IsNullOrWhiteSpace(error.Key))
                        {
                            sb.AppendLine($"{error.Key}:");
                        }

                        foreach (var message in error.Value.Take(error.Value.Count - 1))
                        {
                            sb.AppendLine($" - {message}");
                        }

                        sb.Append($" - {error.Value.Last()}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}