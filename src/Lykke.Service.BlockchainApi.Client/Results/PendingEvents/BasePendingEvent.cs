using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results.PendingEvents
{
    [PublicAPI]
    public abstract class BasePendingEvent
    {
        public Guid OperationId { get; }

        public DateTime Timestamp { get; }

        public string AssetId { get; }

        public decimal Amount { get; }

        protected BasePendingEvent(BasePendingEventContract apiResponse, int assetAccuracy)
        {
            if (apiResponse == null)
            {
                throw new ResultValidationException("API response is null");
            }
            if (assetAccuracy <= 0 || assetAccuracy > 28)
            {
                throw new ResultValidationException("Asset accuracy should be number in the range [1..28]", assetAccuracy);
            }
            if (apiResponse.OperationId == Guid.Empty)
            {
                throw new ResultValidationException("Operation ID should be not empty", apiResponse.OperationId);
            }
            if (apiResponse.Timestamp.Kind != DateTimeKind.Utc)
            {
                throw new ResultValidationException("Timestamp kind should be UTC", apiResponse.Timestamp.Kind);
            }
            if (string.IsNullOrWhiteSpace(apiResponse.AssetId))
            {
                throw new ResultValidationException("Asset ID is required", apiResponse.AssetId);
            }
            if (string.IsNullOrWhiteSpace(apiResponse.Amount))
            {
                throw new ResultValidationException("Amount is required", apiResponse.Amount);
            }
            if (!apiResponse.Amount.All(char.IsNumber))
            {
                throw new ResultValidationException("Amount should be string filled with numbers only", apiResponse.Amount);
            }
            if (!decimal.TryParse(apiResponse.Amount, out var decimalAmount))
            {
                throw new ResultValidationException("Amount can't be converted to the decimal", apiResponse.Amount);
            }

            OperationId = apiResponse.OperationId;
            Timestamp = apiResponse.Timestamp;
            AssetId = apiResponse.AssetId;
            Amount = decimalAmount / (decimal)Math.Pow(10, assetAccuracy);
        }
    }
}
