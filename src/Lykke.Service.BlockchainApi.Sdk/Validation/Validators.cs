using System.Text;
using System.Text.RegularExpressions;
using Common;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using CommonUtils = Common.Utils;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public static class Validators
    {
        public const string MustNotContainInvalidAzureSymbols = "Must not contain invalid Azure symbols and must be less than 512 bytes in size";
        public const string MustBeBase64Encoded = "Must be base64 encoded";

        public static Regex AzureKeyInvalidSymbolsRegex = new Regex(@"[\/\\#?\n\r\t\u0000-\u001F\u007F-\u009F]", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static bool ValidateBase64(string value)
        {
            try
            {
                return !string.IsNullOrEmpty(value.Base64ToString());
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidateAzureContinuation(string value)
        {
            if (value == null)
            {
                return true;
            }

            try
            {
                JsonConvert.DeserializeObject<TableContinuationToken>(CommonUtils.HexToString(value));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidateAzureKey(string value)
        {
            return
                !string.IsNullOrEmpty(value) &&
                !AzureKeyInvalidSymbolsRegex.IsMatch(value) &&
                // we use compound keys, so restrict length only to half of allowed 1 kB,
                // that "ought to be enough for anybody"
                Encoding.UTF8.GetBytes(value).Length < 512;
        }
    }
}