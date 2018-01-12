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
                var balance = Conversions.CoinsFromContract(contract.Balance, assetAccuracy);

                if (balance <= 0)
                {
                    throw new ResultValidationException("Balance should be positive number", balance);
                }
            }
            catch (ConversionException ex)
            {
                throw new ResultValidationException("Failed to parse balance", contract.Balance, ex);
            }
        }
    }
}
