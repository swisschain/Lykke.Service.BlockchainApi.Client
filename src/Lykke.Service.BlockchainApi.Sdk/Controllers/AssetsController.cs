using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Assets;
using Lykke.Service.BlockchainApi.Sdk.Domain.Assets;
using Lykke.Service.BlockchainApi.Sdk.Models;
using Lykke.Service.BlockchainApi.Sdk.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/assets")]
    public class AssetsController : ControllerBase
    {
        readonly AssetRepository _assets;

        public AssetsController(AssetRepository assets) => _assets = assets;
        
        [HttpGet]
        public async Task<ActionResult<PaginationResponse<AssetContract>>> Get(int take, string continuation)
        {
            if (take <= 0)
            {
                return BadRequest(BlockchainErrorResponse.Create("'take' must be grater than zero"));
            }

            if (!Validators.ValidateAzureContinuation(continuation))
            {
                return BadRequest(BlockchainErrorResponse.Create("'continuation' must be null or valid Azure continuation token"));
            }

            var chunk = await _assets.GetAsync(take, continuation);

            return PaginationResponse.From(
                chunk.continuation, 
                chunk.items.Select(e => e.ToContract())
                           .ToList()
            );
        }

        [HttpGet("{assetId}")]
        public async Task<ActionResult<AssetResponse>> Get(string assetId)
        {
            var asset = await _assets.GetAsync(assetId);
            if (asset != null)
                return asset.ToResponse();
            else
                return NoContent();
        }

        [HttpPost]
        public async Task Create(CreateAssetRequest request) =>
            await _assets.UpsertAsync(request.AssetId, request.Address, request.Name, request.Accuracy);
    }
}