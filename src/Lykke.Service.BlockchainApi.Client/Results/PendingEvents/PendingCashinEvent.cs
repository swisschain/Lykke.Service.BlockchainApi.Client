using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results.PendingEvents
{
    [PublicAPI]
    public class PendingCashinEvent : BasePendingEvent
    {
        public PendingCashinEvent(BasePendingEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
        }
    }
}