using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public class PendingCashoutStartedEvent : BasePendingEvent
    {
        public string ToAddress { get; set; }

        public string TransactionHash { get; set; }

        public PendingCashoutStartedEvent(PendingCashoutStartedEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
            ToAddress = apiResponse.ToAddress;
            TransactionHash = apiResponse.TransactionHash;
        }
    }
}