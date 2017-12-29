using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results.PendingEvents
{
    [PublicAPI]
    public class PendingCashinEvent : BasePendingEvent
    {
        public string Address { get; }

        public PendingCashinEvent(PendingCashinEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
            if (string.IsNullOrWhiteSpace(apiResponse.Address))
            {
                throw new ResultValidationException("Address is required", apiResponse.Address);
            }

            Address = apiResponse.Address;
        }
    }
}
