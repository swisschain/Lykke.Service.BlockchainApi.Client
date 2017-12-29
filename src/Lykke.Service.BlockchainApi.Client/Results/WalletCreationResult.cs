using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public class WalletCreationResult
    {
        public string Address { get; }

        public WalletCreationResult(WalletCreationResponse apiResponse)
        {
            if (apiResponse == null)
            {
                throw new ResultValidationException("API response is required");
            }
            if (string.IsNullOrWhiteSpace(apiResponse.Address))
            {
                throw new ResultValidationException("Address is required", apiResponse.Address);
            }

            Address = apiResponse.Address;
        }
    }
}
