using Common;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public static class Validators
    {
        public static bool BeBase64Encoded(string value)
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
    }
}