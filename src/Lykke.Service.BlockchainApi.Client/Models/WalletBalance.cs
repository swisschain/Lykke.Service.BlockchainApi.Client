using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Wallet balance
    /// </summary>
    [PublicAPI]
    public class WalletBalance
    {
        /// <summary>
        /// Wallet address
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Asset IDs
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// Wallet balance
        /// </summary>
        public decimal Balance { get; }

        public WalletBalance(WalletBalanceContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Contract value is required");
            }
            if (string.IsNullOrWhiteSpace(contract.Address))
            {
                throw new ResultValidationException("Address is required", contract.Address);
            }
            if (string.IsNullOrWhiteSpace(contract.AssetId))
            {
                throw new ResultValidationException("Asset ID is required", contract.AssetId);
            }

            Address = contract.Address;
            AssetId = contract.AssetId;

            try
            {
                Balance = Conversions.CoinsFromContract(contract.Balance, assetAccuracy);
            }
            catch (ConversionException ex)
            {
                throw new ResultValidationException("Failed to parse balance", contract.Balance, ex);
            }
        }
    }
}
