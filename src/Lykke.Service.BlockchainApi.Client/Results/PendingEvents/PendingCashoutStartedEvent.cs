using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results.PendingEvents
{
    [PublicAPI]
    public class PendingCashoutStartedEvent : BasePendingEvent
    {
        public string FromAddress { get; }

        public string ToAddress { get; }

        public string TransactionHash { get; }

        public PendingCashoutStartedEvent(PendingCashoutStartedEventContract apiResponse, int assetAccuracy) : 
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
            if (string.IsNullOrWhiteSpace(apiResponse.TransactionHash))
            {
                throw new ResultValidationException("Transaction hash is required", apiResponse.TransactionHash);
            }

            FromAddress = apiResponse.FromAddress;
            ToAddress = apiResponse.ToAddress;
            TransactionHash = apiResponse.TransactionHash;
        }
    }
}
