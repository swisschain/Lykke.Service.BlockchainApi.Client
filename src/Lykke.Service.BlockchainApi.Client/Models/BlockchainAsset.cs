using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Assets;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    /// <summary>
    /// Blockchain asset
    /// </summary>
    [PublicAPI]
    public class BlockchainAsset
    {
        /// <summary>
        /// AssetId
        /// </summary>
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

        public BlockchainAsset(AssetContract contract)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Contract value is required");
            }
            if (string.IsNullOrWhiteSpace(contract.AssetId))
            {
                throw new ResultValidationException("Asset ID is required", contract.AssetId);
            }
            if (string.IsNullOrWhiteSpace(contract.Name))
            {
                throw new ResultValidationException("Name is required", contract.Name);
            }
            if (contract.Accuracy < 0 || contract.Accuracy > 28)
            {
                throw new ResultValidationException("Accuracy should be number in the range [0..28]", contract.Accuracy);
            }

            AssetId = contract.AssetId;
            Address = contract.Address;
            Name = contract.Name;
            Accuracy = contract.Accuracy;
        }
    }
}
