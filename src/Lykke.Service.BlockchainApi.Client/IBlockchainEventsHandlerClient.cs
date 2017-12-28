using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract.Responses;

namespace Lykke.Service.BlockchainApi.Client
{
    [PublicAPI]
    public interface IBlockchainEventsHandlerClient : IDisposable
    {
        /// <summary>
        /// Checks if Blockchain API service alive and returns some common metadata
        /// </summary>
        Task<IsAliveResponse> GetIsAliveAsync();


        #region Assets

        Task<AssetsListResponse> GetAssetsAsync();
        Task<AssetResponse> GetAssetAsync(string assetId);

        #endregion


        #region Addresses

        Task<bool> IsAddressValidAsync(string address);

        #endregion


        #region Wallets

        Task<WalletCreationResponse> CreateWalletAsync();

        Task CashoutFromWalletAsync(string address, string toAddress, string assetId, decimal amount, IReadOnlyList<string> signers);

        #endregion


        #region Pending events

        Task<PendingEventsResponse<Results.PendingCashinEvent>> GetPendingCashinEventsAsync(int maxEventsNumber);

        Task<PendingEventsResponse<Results.PendingCashoutStartedEvent>> GetPendingCashoutStartedEventsAsync(int maxEventsNumber);

        Task<PendingEventsResponse<Results.PendingCashoutCompletedEvent>> GetPendingCashoutCompletedEventsAsync(int maxEventsNumber);

        Task<PendingEventsResponse<Results.PendingCashoutFailedEvent>> GetPendingCashoutFailedEventsAsync(int maxEventsNumber);

        Task RemovePendingCashinEventsAsync(IReadOnlyList<string> operationIds);
        Task RemovePendingCashoutStartedEventsAsync(IReadOnlyList<string> operationIds);
        Task RemovePendingCashoutCompletedEventsAsync(IReadOnlyList<string> operationIds);
        Task RemovePendingCashoutFailedEventsAsync(IReadOnlyList<string> operationIds);

        #endregion
    }
}