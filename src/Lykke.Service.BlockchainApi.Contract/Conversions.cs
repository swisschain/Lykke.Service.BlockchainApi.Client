using System;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract
{
    /// <summary>
    /// Provides contract conversions
    /// </summary>
    [PublicAPI]
    public static class Conversions
    {
        /// <summary>
        /// Converts coins amount from the <see cref="decimal"/> to the 
        /// contract string representation as the integer aligned to the
        /// asset accuracy
        /// </summary>
        /// <exception cref="ConversionException">Throws if conversion can't be done</exception>
        public static string CoinsToContract(decimal value, int assetAccuracy)
        {
            CheckAssetAccuracy(value, assetAccuracy);

            if (value < 0)
            {
                throw new ConversionException("Value can't be negative", typeof(string), typeof(decimal), value);
            }

            try
            {
                var alignedValue = Math.Round(assetAccuracy > 0
                    ? value * (decimal) Math.Pow(10, assetAccuracy)
                    : value);

                return alignedValue.ToString(CultureInfo.InvariantCulture);
            }
            catch (OverflowException ex)
            {
                throw new ConversionException("Failed to align the value", typeof(string), typeof(decimal), value, ex);
            }
        }

        /// <summary>
        /// Converts coins amount from the the contract string representation as 
        /// the integer aligned to the asset accuracy to the <see cref="decimal"/>
        /// </summary>
        /// <exception cref="ConversionException">Throws if conversion can't be done</exception>
        public static decimal CoinsFromContract(string value, int assetAccuracy)
        {
            CheckAssetAccuracy(value, assetAccuracy);
           
            if (value.Length < 1)
            {
                throw new ConversionException("Value should be at least 1 digit in length", typeof(string), typeof(decimal), value);
            }

            if (value.Length > 29)
            {
                throw new ConversionException("Value should not exceed 28 digits in length", typeof(string), typeof(decimal), value);
            }

            if (!value.All(char.IsNumber))
            {
                throw new ConversionException("Value should be string filled with numbers only", typeof(string), typeof(decimal), value);
            }
            
            if (!decimal.TryParse(value, out var decimalAmount))
            {
                throw new ConversionException("Value can't be converted to the decimal", typeof(string), typeof(decimal), value);
            }

            try
            {
                return assetAccuracy > 0
                    ? decimalAmount / (decimal) Math.Pow(10, assetAccuracy)
                    : decimalAmount;
            }
            catch (OverflowException ex)
            {
                throw new ConversionException("Failed to normalize the value", typeof(string), typeof(decimal), value, ex);
            }
        }

        private static void CheckAssetAccuracy(object value, int assetAccuracy)
        {
            if (assetAccuracy < 0 || assetAccuracy > 28)
            {
                throw new ConversionException($"Asset accuracy should be number in the range [0..28], but is [{assetAccuracy}]",
                    typeof(string), typeof(decimal), value);
            }
        }
    }
}
