using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results.PendingEvents
{
    [PublicAPI]
    public class PendingCashoutFailedEvent : BasePendingEvent
    {
        public string ToAddress { get; set; }

        public PendingCashoutFailedEvent(PendingCashoutFailedEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
            if (string.IsNullOrWhiteSpace(apiResponse.ToAddress))
            {
                throw new ResultValidationException("Destination address is required", apiResponse.ToAddress);
            }
    
            ToAddress = apiResponse.ToAddress;
        }
    }
}
