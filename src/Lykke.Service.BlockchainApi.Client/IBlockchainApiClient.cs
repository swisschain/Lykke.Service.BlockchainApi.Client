using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Client.Results;
using Lykke.Service.BlockchainApi.Client.Results.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client
{
    [PublicAPI]
    public interface IBlockchainApiClient : IDisposable
    {
        string HostUrl { get; }

        /// <summary>
        /// Checks if Blockchain API service alive and returns some common metadata
        /// </summary>
        Task<IsAliveResponse> GetIsAliveAsync();


        #region Assets

        Task<BlockchainAssetsList> GetAssetsAsync();
        Task<BlockchainAsset> GetAssetAsync(string assetId);

        #endregion


        #region Addresses

        Task<bool> IsAddressValidAsync(string address);

        #endregion


        #region Wallets

        Task<WalletCreationResult> CreateWalletAsync();

        Task CashoutFromWalletAsync(string address, string toAddress, string assetId, decimal amount, IReadOnlyList<string> signers);

        #endregion


        #region Pending events

        Task<PendingEventsList<PendingCashinEvent>> GetPendingCashinEventsAsync(int maxEventsNumber);

        Task<PendingEventsList<PendingCashoutStartedEvent>> GetPendingCashoutStartedEventsAsync(int maxEventsNumber);

        Task<PendingEventsList<PendingCashoutCompletedEvent>> GetPendingCashoutCompletedEventsAsync(int maxEventsNumber);

        Task<PendingEventsList<PendingCashoutFailedEvent>> GetPendingCashoutFailedEventsAsync(int maxEventsNumber);

        Task RemovePendingCashinEventsAsync(IReadOnlyList<Guid> operationIds);
        Task RemovePendingCashoutStartedEventsAsync(IReadOnlyList<Guid> operationIds);
        Task RemovePendingCashoutCompletedEventsAsync(IReadOnlyList<Guid> operationIds);
        Task RemovePendingCashoutFailedEventsAsync(IReadOnlyList<Guid> operationIds);

        #endregion
    }
}
