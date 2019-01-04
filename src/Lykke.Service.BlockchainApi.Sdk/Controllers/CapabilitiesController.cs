using Lykke.Service.BlockchainApi.Contract.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/capabilities")]
    public class CapabilitiesController : ControllerBase
    {
        [HttpGet]
        public CapabilitiesResponse Get([FromServices] IBlockchainApi api) => api.GetCapabilities();
    }
}