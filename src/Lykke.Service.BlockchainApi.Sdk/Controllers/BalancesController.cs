using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.BlockchainApi.Sdk.Domain;
using Lykke.Service.BlockchainApi.Sdk.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/balances")]
    public class BalancesController : ControllerBase
    {
        readonly AssetRepository _assets;
        readonly DepositWalletRepository _depositWallets;
        readonly IBlockchainApi _api;

        public BalancesController(AssetRepository assets, DepositWalletRepository depositWallets, IBlockchainApi api) => 
            (_assets, _depositWallets, _api) = (assets, depositWallets, api);

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<WalletBalanceContract>>> Get([Range(1, int.MaxValue)] int take, [AzureContinuation] string continuation) 
        {
            IEnumerable<DepositWalletBalanceEntity> balances;

            if (_api.CanGetBalances)
            {
                // to be able to enroll fake transfers properly
                // we keep block number increased by one order of magnitude
                var lastConfirmedBlock = await _api.GetLastConfirmedBlockNumberAsync() * 10;
                var depositWallets = await _depositWallets.GetWalletsAsync(take, continuation);
                var addresses = depositWallets.items.Select(w => w.Address).ToArray();

                continuation = depositWallets.continuation;
                balances = (await _api.GetBalancesAsync(addresses, _assets.GetCachedAsync()))
                    .Select(b => new DepositWalletBalanceEntity(b.Address, b.AssetId)
                    {
                        Amount = b.Amount,
                        BlockNumber = lastConfirmedBlock
                    });
            }
            else
            {
                (balances, continuation) = 
                    await _depositWallets.GetBalanceAsync(take, continuation);
            }

            var assetCache = _assets.GetCachedAsync();
            var result = new List<WalletBalanceContract>();

            foreach (var balance in balances)
            {
                // blockchain may return unknown assets,
                // filter out such items

                var accuracy = (await assetCache(balance.AssetId))?.Accuracy;
                if (accuracy == null)
                {
                    continue;
                }

                result.Add(new WalletBalanceContract
                {
                    Address = balance.Address,
                    AssetId = balance.AssetId,
                    Balance = Conversions.CoinsToContract(balance.Amount, accuracy.Value),
                    Block = balance.BlockNumber
                });
            }

            return PaginationResponse.From(continuation, result);
        }
    
        [HttpPost("{address}/observation")]
        public async Task<ActionResult> Observe(string address)
        {
            if (await _depositWallets.TryObserveAsync(address))
            {   
                await _api.ObserveAddressAsync(address);
                return Ok();
            }
            else
            {
                return Conflict($"Address {address} is already observed");
            }
        }

        [HttpDelete("{address}/observation")]
        public async Task<ActionResult> DeleteObservation(string address)
        {
            if (await _depositWallets.TryDeleteObservationAsync(address))
            {
                await _api.DeleteAddressObservationAsync(address);
                return Ok();
            }
            else
            {
                return NoContent();
            }
        }
    }
}