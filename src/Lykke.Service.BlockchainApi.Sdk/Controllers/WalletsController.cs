using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/wallets")]
    public class WalletsController : ControllerBase
    {
        [HttpPost]
        public async Task<WalletResponse> CreateWallet([FromServices] IBlockchainSignService signService) =>
            await signService.CreateWalletAsync();
    }
}
