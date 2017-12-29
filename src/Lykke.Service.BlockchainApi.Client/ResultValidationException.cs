using System;
using Common;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client
{
    [PublicAPI]
    public class ResultValidationException : Exception
    {
        public ResultValidationException(string message) :
            base(message)
        {
        }

        public ResultValidationException(string message, object actualValue) :
            base(BuildMessage(message, actualValue))
        {
        }

        public ResultValidationException(string message, Exception inner) :
            base(message, inner)
        {
        }

        private static string BuildMessage(string message, object actualValue)
        {
            return actualValue == null 
                ? $"{message}. Actual value: is null" 
                : $"{message}. Actual value: [{actualValue}]";
        }
    }
}
