using Lykke.Service.BlockchainApi.Contract.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/constants")]
    public class ConstantsController : ControllerBase
    {
        [HttpGet]
        public ConstantsResponse Get([FromServices] IBlockchainApi api) => api.GetConstants();
    }
}