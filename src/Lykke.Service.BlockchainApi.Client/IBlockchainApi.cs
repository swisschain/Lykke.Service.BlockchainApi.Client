using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract.Requests;
using Lykke.Service.BlockchainApi.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    internal interface IBlockchainApi
    {
        [Get("/api/isalive")]
        Task<IsAliveResponse> GetIsAliveAsync();

        #region Assets

        [Get("/api/assets")]
        Task<AssetsListResponse> GetAssetsAsync();

        [Get("/api/assets/{assetId}")]
        Task<AssetResponse> GetAssetAsync(string assetId);

        #endregion


        #region Addresses

        [Get("/api/addresses/{address}/is-valid")]
        Task<AddressValidationResponse> IsAddressValidAsync(string address);

        #endregion
        

        #region Wallets

        [Post("/api/wallets")]
        Task<WalletCreationResponse> CreateWalletAsync();

        [Post("/api/wallets/{address}/cashout")]
        Task CashoutFromWalletAsync(string address, [Body] CashoutFromWalletRequest body);

        #endregion


        #region Events

        [Get("/api/pending-events/cashin")]
        Task<PendingEventsResponse<PendingCashinEventContract>> GetPendingCashinEventsAsync(int maxEventsNumber);

        [Get("/api/pending-events/cashout-started")]
        Task<PendingEventsResponse<PendingCashoutStartedEventContract>> GetPendingCashoutStartedEventsAsync(int maxEventsNumber);

        [Get("/api/pending-events/cashout-completed")]
        Task<PendingEventsResponse<PendingCashoutCompletedEventContract>> GetPendingCashoutCompletedEventsAsync(int maxEventsNumber);

        [Get("/api/pending-events/cashout-failed")]
        Task<PendingEventsResponse<PendingCashoutFailedEventContract>> GetPendingCashoutFailedEventsAsync(int maxEventsNumber);

        [Delete("/api/pending-events/cashin")]
        Task RemovePendingCashinEventsAsync([Body] RemovePendingEventsRequest body);

        [Delete("/api/pending-events/cashout-started")]
        Task RemovePendingCashoutStartedEventsAsync([Body] RemovePendingEventsRequest body);

        [Delete("/api/pending-events/cashout-completed")]
        Task RemovePendingCashoutCompletedEventsAsync([Body] RemovePendingEventsRequest body);

        [Delete("/api/pending-events/cashout-failed")]
        Task RemovePendingCashoutFailedEventsAsync([Body] RemovePendingEventsRequest body);

        #endregion
    }
}
