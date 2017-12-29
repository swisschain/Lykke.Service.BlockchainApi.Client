using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Requests
{
    [PublicAPI]
    public class CashoutFromWalletRequest
    {
        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("signers")]
        public IReadOnlyList<string> Signers { get; set; }
    }
}
