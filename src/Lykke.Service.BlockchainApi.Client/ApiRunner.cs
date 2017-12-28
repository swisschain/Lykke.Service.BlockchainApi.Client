using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Polly;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    internal class ApiRunner
    {
        private readonly ILog _log;
        private readonly int _defaultRetriesCount;

        public ApiRunner(ILog log, int defaultRetriesCount = int.MaxValue)
        {
            _log = log;
            _defaultRetriesCount = defaultRetriesCount;
        }

        public async Task RunAsync(Func<Task> method)
        {
            try
            {
                await method();
            }
            catch (ApiException ex)
            {
                throw new ErrorResponseException(ex.GetContentAs<ErrorResponse>(), ex);
            }
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> method)
        {
            try
            {
                return await method();
            }
            catch (ApiException ex)
            {
                throw new ErrorResponseException(ex.GetContentAs<ErrorResponse>(), ex);
            }
        }

        public Task RunWithRetriesAsync(Func<Task> method, int? retriesCount = null)
        {
            return Policy
                .Handle<Exception>(FilterRetryExceptions)
                .WaitAndRetryAsync(
                    retriesCount ?? _defaultRetriesCount,
                    GetRetryDelay,
                    (ex, timeSpan) => WriteRetryErrorAsync(method, timeSpan, ex))
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        await method();
                    }
                    catch (ApiException ex)
                    {
                        var errorResponse = ex.GetContentAs<ErrorResponse>();

                        if (errorResponse != null)
                        {
                            throw new ErrorResponseException(errorResponse, ex);
                        }

                        throw;
                    }
                });
        }

        public Task<T> RunWithRetriesAsync<T>(Func<Task<T>> method, int? retriesCount = null)
        {

            return Policy
                .Handle<Exception>(FilterRetryExceptions)
                .WaitAndRetryAsync(
                    retriesCount ?? _defaultRetriesCount,
                    GetRetryDelay,
                    (ex, timeSpan) => WriteRetryErrorAsync(method, timeSpan, ex))
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        return await method();
                    }
                    catch (ApiException ex)
                    {
                        var errorResponse = ex.GetContentAs<ErrorResponse>();

                        if (errorResponse != null)
                        {
                            throw new ErrorResponseException(errorResponse, ex);
                        }

                        throw;
                    }
                });
        }

        private Task WriteRetryErrorAsync(Delegate method, TimeSpan timeSpan, Exception ex)
        {
            var process = $"Blockchain events handler HTTP API request - {method.Method.Name}";

            if (ex is ErrorResponseException errorResponseException)
            {
                return _log.WriteWarningAsync(
                    process,
                    GetExceptionMessage(errorResponseException),
                    $"was failed. Will be retried in {timeSpan}");
            }

            return _log.WriteWarningAsync(
                process,
                "",
                $"Blockchain events handler HTTP API call was failed. Will be retried in {timeSpan}",
                ex);
        }

        private static string GetExceptionMessage(Exception exception)
        {
            var ex = exception;
            var sb = new StringBuilder();

            while (true)
            {
                sb.AppendLine(ex.Message);

                ex = ex.InnerException;

                if (ex == null)
                {
                    return sb.ToString();
                }

                sb.Append(" -> ");
            }
        }

        private static bool FilterRetryExceptions(Exception ex)
        {
            if (ex.InnerException is ApiException apiException)
            {
                return apiException.StatusCode == HttpStatusCode.InternalServerError ||
                        apiException.StatusCode == HttpStatusCode.BadGateway ||
                        apiException.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        apiException.StatusCode == HttpStatusCode.GatewayTimeout;
            }

            return true;
        }

        private static TimeSpan GetRetryDelay(int retryAttempt)
        {
            if (retryAttempt < 3)
            {
                return TimeSpan.FromMilliseconds(500 * retryAttempt);
            }

            if (retryAttempt < 8)
            {
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 2));
            }

            return TimeSpan.FromMinutes(1);
        }
    }
}