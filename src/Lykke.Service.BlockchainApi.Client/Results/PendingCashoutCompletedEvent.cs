using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public class PendingCashoutCompletedEvent : BasePendingEvent
    {
        public string ToAddress { get; }

        public string TransactionHash { get; }

        public PendingCashoutCompletedEvent(PendingCashoutCompletedEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
            ToAddress = apiResponse.ToAddress;
            TransactionHash = apiResponse.TransactionHash;
        }
    }
}