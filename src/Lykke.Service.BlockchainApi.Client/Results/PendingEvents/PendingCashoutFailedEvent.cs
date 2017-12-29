using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results.PendingEvents
{
    [PublicAPI]
    public class PendingCashoutFailedEvent : BasePendingEvent
    {
        public string FromAddress { get; }

        public string ToAddress { get; }

        public PendingCashoutFailedEvent(PendingCashoutFailedEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
            if (string.IsNullOrWhiteSpace(apiResponse.FromAddress))
            {
                throw new ResultValidationException("Source address is required", apiResponse.FromAddress);
            }
            if (string.IsNullOrWhiteSpace(apiResponse.ToAddress))
            {
                throw new ResultValidationException("Destination address is required", apiResponse.ToAddress);
            }

            FromAddress = apiResponse.FromAddress;
            ToAddress = apiResponse.ToAddress;
        }
    }
}
