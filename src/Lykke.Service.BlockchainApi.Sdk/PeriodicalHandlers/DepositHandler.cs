using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Sdk.Domain.Assets;
using Lykke.Service.BlockchainApi.Sdk.Domain.DepositWallets;
using Lykke.Service.BlockchainApi.Sdk.Domain.Operations;
using Lykke.Service.BlockchainApi.Sdk.Domain.State;
using Lykke.Service.BlockchainApi.Sdk.Validation;

namespace Lykke.Service.BlockchainApi.Sdk.PeriodicalHandlers
{
    public class DepositHandler<TState> : TimerPeriod
    {
        readonly DepositWalletRepository _depositWallets;
        readonly OperationRepository _operations;
        readonly AssetRepository _assets;
        readonly StateRepository<TState> _state;
        readonly IBlockchainJob<TState> _job;
        readonly IChaosKitty _chaosKitty;
        readonly ILog _log;

        public DepositHandler(TimeSpan period, ILogFactory logFactory, DepositWalletRepository depositWallets, OperationRepository operations, AssetRepository assets,
            StateRepository<TState> state, IBlockchainJob<TState> job, IChaosKitty chaosKitty = null) : base(period, logFactory, nameof(DepositHandler<TState>))
        {
            _depositWallets = depositWallets;
            _operations = operations;
            _assets = assets;
            _state = state;
            _job = job;
            _chaosKitty = chaosKitty;
            _log = logFactory.CreateLog(this);
        }

        public override async Task Execute()
        {
            var currentState = await _state.GetAsync();
            var state = currentState != null ? currentState.State : default;
            var trace = await _job.TraceDepositsAsync(state, async assetId => await _assets.GetAsync(assetId));
            var operationHashes = new HashSet<string>();
            var deposits = new List<BlockchainAction>();

            foreach (var action in trace.actions)
            {
                // to be able to enroll fake transfers properly
                // we keep block number increased by one order of magnitude
                action.BlockNumber *= 10;

                // changes made by our operations are accounted by API service,
                // so here we enroll external deposits only
                if (operationHashes.Contains(action.TransactionHash))
                    continue;
                else if (await _operations.GetOperationIndexAsync(action.TransactionHash) != null)
                    operationHashes.Add(action.TransactionHash);
                else
                    deposits.Add(action);
            }

            // filter out actions with invalid Azure symbols
            deposits = deposits
                .Where(a =>
                {
                    var containsInvalidSymbols = 
                        !Validators.ValidateAzureKey(a.ActionId) || 
                        !Validators.ValidateAzureKey(a.Address) || 
                        !Validators.ValidateAzureKey(a.AssetId) || 
                        !Validators.ValidateAzureKey(a.TransactionHash);

                    if (containsInvalidSymbols)
                        _log.Warning("Invalid Azure symbols and/or length in action", context: a);

                    return !containsInvalidSymbols;
                })
                .ToList();

            await _depositWallets.EnrollIfObservedAsync(deposits);

            _chaosKitty?.Meow($"{nameof(Execute)}_Enroll");

            await _state.UpsertAsync(trace.state);

            _chaosKitty?.Meow($"{nameof(Execute)}_UpdateState");
        }
    }
}