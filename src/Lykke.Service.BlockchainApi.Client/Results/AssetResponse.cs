using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public class BlockchainAsset
    {
        public string AssetId { get; }

        /// <summary>
        /// Optional
        /// </summary>
        [CanBeNull]
        public string Address { get; }

        public string Name { get; }

        /// <summary>
        /// Max number of significant decimal digits to the right of the decimal point in the asset amount
        /// </summary>
        public int Accuracy { get; }

        public BlockchainAsset(AssetResponse apiResponse)
        {
            if (apiResponse == null)
            {
                throw new ResultValidationException("API response is required");
            }
            if (string.IsNullOrWhiteSpace(apiResponse.AssetId))
            {
                throw new ResultValidationException("Asset ID is required", apiResponse.AssetId);
            }
            if (string.IsNullOrWhiteSpace(apiResponse.Name))
            {
                throw new ResultValidationException("Name is required", apiResponse.Name);
            }
            if (apiResponse.Accuracy <= 0 || apiResponse.Accuracy > 28)
            {
                throw new ResultValidationException("Accuracy should be number in the range [1..28]", apiResponse.Accuracy);
            }

            AssetId = apiResponse.AssetId;
            Address = apiResponse.Address;
            Name = apiResponse.Name;
            Accuracy = apiResponse.Accuracy;
        }
    }
}
