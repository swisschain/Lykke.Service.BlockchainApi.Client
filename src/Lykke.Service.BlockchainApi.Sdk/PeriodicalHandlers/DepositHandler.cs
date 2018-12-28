using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Sdk.Domain;

namespace Lykke.Service.BlockchainApi.Sdk.PeriodicalHandlers
{
    public class DepositHandler<T> : TimerPeriod
    {
        readonly DepositWalletRepository _depositWallets;
        readonly OperationRepository _operations;
        readonly AssetRepository _assets;
        readonly StateRepository<T> _state;
        readonly IBlockchainJob<T> _job;

        public DepositHandler(TimeSpan period, ILogFactory logFactory, DepositWalletRepository depositWallets, OperationRepository operations, AssetRepository assets,
            StateRepository<T> state, IBlockchainJob<T> job) : base(period, logFactory, nameof(DepositHandler<T>))
        {
            _depositWallets = depositWallets;
            _operations = operations;
            _assets = assets;
            _state = state;
            _job = job;
        }

        public override async Task Execute()
        {
            var currentState = await _state.GetAsync();
            var state = currentState?.State ?? default(T);
            var trace = await _job.TraceDepositsAsync(state, _assets.GetCachedAsync());
            var operationHashes = new HashSet<string>();
            var deposits = new List<BlockchainAction>();

            foreach (var action in trace.actions)
            {
                // to be able to enroll fake transfers properly
                // we keep block number increased by one order of magnitude
                action.BlockNumber *= 10;

                // changes made by our operations are accounted by API service,
                // so here we enroll external deposits only
                if (operationHashes.Contains(action.TransactionHash) || await _operations.GetOperationIndexAsync(action.TransactionHash) != null)
                    operationHashes.Add(action.TransactionHash);
                else
                    deposits.Add(action);
            }

            await _depositWallets.EnrollIfObservedAsync(deposits);

            await _state.UpsertAsync(trace.state);
        }
    }
}
