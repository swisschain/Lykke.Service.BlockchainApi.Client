using System;

namespace Lykke.Service.BlockchainApi.Contract
{
    /// <summary>
    /// Conversion exception
    /// </summary>
    public class ConversionException : Exception
    {
        /// <summary>
        /// Conversion exception
        /// </summary>
        public ConversionException(string message, Type fromType, Type toType, object sourceValue, Exception innerException = null) :
            base(BuildMessage(message, fromType, toType, sourceValue), innerException)
        {
        }

        private static string BuildMessage(string message, Type fromType, Type toType, object sourceValue)
        {
            return $"Conversion of the [{sourceValue}] value from the {fromType.FullName} type to the {toType.FullName} type failed: {message}";
        }
    }
}
