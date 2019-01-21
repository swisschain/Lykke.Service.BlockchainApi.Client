using System;
using System.Linq;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Common;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Sdk.Controllers
{
    [ApiController]
    [Route("/api/sign")]
    public class SignController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<SignedTransactionResponse>> SignTransaction(SignTransactionRequest request,
            [FromServices] IBlockchainSignService signService)
        {
            if (request.PrivateKeys.Count == 0 || 
                request.PrivateKeys.Any(k => !signService.ValidatePrivateKey(k)))
            {
                return BadRequest(BlockchainErrorResponse.Create("Invalid private key(s)"));
            }

            var tx = request.TransactionContext.Base64ToString();

            if (tx == Constants.DUMMY_TX && 
                request.PrivateKeys.Any(k => !signService.ValidateHotWalletPrivateKey(k)))
            {
                return BadRequest(BlockchainErrorResponse.Create("Invalid private key(s)"));
            }

            var result = (
                hash: Constants.DUMMY_HASH,
                signedTransaction: Constants.DUMMY_TX
            );

            if (tx != Constants.DUMMY_TX)
            {
                try
                {
                    result = await signService.SignTransactionAsync(tx, request.PrivateKeys);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(BlockchainErrorResponse.Create(ex.ToAsyncString()));
                }
            }

            return new SignedTransactionResponse()
            {
                SignedTransaction = JsonConvert.SerializeObject(result).ToBase64()
            };
        }
    }
}
