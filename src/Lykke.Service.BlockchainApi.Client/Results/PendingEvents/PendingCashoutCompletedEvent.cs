using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results.PendingEvents
{
    [PublicAPI]
    public class PendingCashoutCompletedEvent : BasePendingEvent
    {
        public string ToAddress { get; }

        public string TransactionHash { get; }

        public PendingCashoutCompletedEvent(PendingCashoutCompletedEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
            if (string.IsNullOrWhiteSpace(apiResponse.ToAddress))
            {
                throw new ResultValidationException("Destination address is required", apiResponse.ToAddress);
            }
            if (string.IsNullOrWhiteSpace(apiResponse.TransactionHash))
            {
                throw new ResultValidationException("Transaction hash is required", apiResponse.TransactionHash);
            }

            ToAddress = apiResponse.ToAddress;
            TransactionHash = apiResponse.TransactionHash;
        }
    }
}
