using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.BlockchainApi.Sdk.Models;
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
                return BadRequest("Invalid private key(s)");
            }

            if (request.TransactionContext == Constants.DUMMY_TX && 
                request.PrivateKeys.Any(k => !signService.ValidateHotWalletPrivateKey(k)))
            {
                return BadRequest("Invalid private key(s)");
            }

            var result = (
                hash: DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString("D"),
                signedTransaction: Constants.DUMMY_TX
            );

            if (request.TransactionContext != Constants.DUMMY_TX)
            {
                try
                {
                    result = await signService.SignTransactionAsync(request.TransactionContext, request.PrivateKeys);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return new SignedTransactionResponse()
            {
                SignedTransaction = JsonConvert.SerializeObject(result).ToBase64()
            };
        }
    }
}