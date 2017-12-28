using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public abstract class BasePendingEvent
    {
        public string OperationId { get; }

        public DateTime Timestamp { get; }

        public string AssetId { get; }

        public decimal Amount { get; }

        protected BasePendingEvent(BasePendingEventContract apiResponse, int assetAccuracy)
        {
            OperationId = apiResponse.OperationId;
            Timestamp = apiResponse.Timestamp;
            AssetId = apiResponse.AssetId;
            Amount = decimal.Parse(apiResponse.Amount) / assetAccuracy;
        }
    }
}