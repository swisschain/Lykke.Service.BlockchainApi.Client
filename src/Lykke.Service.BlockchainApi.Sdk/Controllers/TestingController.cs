using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Testing;
using Lykke.Service.BlockchainApi.Sdk.Domain.Assets;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/testing")]
    public class TestingController : ControllerBase
    {
        readonly IBlockchainApi _api;
        readonly AssetRepository _assets;

        public TestingController(IBlockchainApi api, AssetRepository assets)
        {
            _api = api;
            _assets = assets;
        }

        [HttpPost("transfers")]
        public async Task<ActionResult> Transfer(TestingTransferRequest request)
        {
            var caps = _api.GetCapabilities();

            if (!caps.IsTestingTransfersSupported.HasValue ||
                !caps.IsTestingTransfersSupported.Value)
            {
                return StatusCode(501);
            }               

            var asset = await _assets.GetAsync(request.AssetId);

            if (asset == null)
            {
                return BadRequest(BlockchainErrorResponse.Create("Unknown asset"));
            }

            var amount = 0M;

            try
            {
                amount = Conversions.CoinsFromContract(request.Amount, asset.Accuracy);
            }
            catch (ConversionException)
            {
                return BadRequest(BlockchainErrorResponse.Create("Invalid amount format"));
            }

            if (amount < 0 || !_api.AddressIsValid(request.FromAddress) || !_api.AddressIsValid(request.ToAddress))
            {
                return BadRequest(BlockchainErrorResponse.Create("Invalid address(es) and/or negative amount"));
            }

            var result = await _api.TestingTransfer(
                request.FromAddress, 
                request.FromPrivateKey, 
                request.ToAddress, 
                asset,
                amount);

            return Ok(result);
        }
    }
}