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

        /// <summary>
        /// Incremental ID of the moment, when balance is updated. It should be the same sequence
        /// as for nonce in the <see cref="IBlockchainApiClient.GetBroadcastedSingleTransactionAsync"/> response
        /// For the most blockchains it could be the block number.
        /// </summary>
        public long Nonce { get; }

        public WalletBalance(WalletBalanceContract contract, int assetAccuracy)
        {
            if (contract == null)
            {
                throw new ResultValidationException("Wallet not found");
            }
            if (string.IsNullOrWhiteSpace(contract.Address))
            {
                throw new ResultValidationException("Address is required", contract.Address);
            }
            if (string.IsNullOrWhiteSpace(contract.AssetId))
            {
                throw new ResultValidationException("Asset ID is required", contract.AssetId);
            }
            if (Nonce == 0)
            {
                throw new ResultValidationException("Nonce is required", contract.Nonce);
            }

            Address = contract.Address;
            AssetId = contract.AssetId;
            
            try
            {
                Balance = Conversions.CoinsFromContract(contract.Balance, assetAccuracy);

                if (Balance <= 0)
                {
                    throw new ResultValidationException("Balance should be positive number", contract.Balance);
                }
            }
            catch (ConversionException ex)
            {
                throw new ResultValidationException("Failed to parse balance", contract.Balance, ex);
            }

            Nonce = Nonce;
        }
    }
}
