using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    [PublicAPI]
    public class HistoricalTransaction : BaseTransaction
    {
        /// <summary>
        /// Transaction hash as base64 string.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; }

        public HistoricalTransaction(HistoricalTransactionContract contract, int assetAccuracy) : 
            base(contract, assetAccuracy)
        {
            if (string.IsNullOrWhiteSpace(contract.Hash))
            {
                throw new ResultValidationException("Hash is required", contract.Hash);
            }

            Hash = contract.Hash;
        }
    }
}
