using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Building transaction input
    /// </summary>
    [PublicAPI]
    public class BuildingTransactionInput
    {
        /// <summary>
        /// Source address
        /// 
        /// For the blockchains with address mapping,
        /// this could be virtual address.
        /// </summary>
        public string FromAddress { get; }

        /// <summary>
        /// Source address context.
        /// Can be emoty
        /// </summary>
        public string FromAddressContext { get; }

        /// <summary>
        /// Amount to transfer from the <see cref="FromAddress"/> or actual amount, 
        /// which is transferred from the <see cref="FromAddress"/>, depending on the context.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Building transaction input
        /// </summary>
        protected BuildingTransactionInput(string fromAddress, string fromAddressContext, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(FromAddress))
            {
                throw new ArgumentException(nameof(fromAddress), "Source address is required");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount should be positive number");
            }

            FromAddress = fromAddress;
            FromAddressContext = fromAddressContext;
            Amount = amount;
        }

        /// <summary>
        /// Converts transaction input to the contract DTO
        /// </summary>
        public BuildingTransactionInputContract ToContract(int assetAccuracy)
        {
            return new BuildingTransactionInputContract
            {
                FromAddress = FromAddress,
                FromAddressContext = FromAddressContext,
                Amount = Conversions.CoinsToContract(Amount, assetAccuracy)
            };
        }
    }
}
