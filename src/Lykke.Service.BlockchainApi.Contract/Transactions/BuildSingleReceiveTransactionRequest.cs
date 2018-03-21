using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Transactions
{
    /// <summary>
    /// Receive transaction building parameters.
    /// Request body for the:
    ///     [POST] /api/transactions/single/receive
    /// </summary>
    /// <remarks>
    /// Should build not signed “receive” transaction to receive funds previously sent 
    /// from the single source to the single destination. If the receive transaction 
    /// with the specified operationId has already been built by the 
    /// [POST] /api/transactions/single/receive call, it should be ignored and regular
    /// response (as in the first request) should be returned. This endpoint should be 
    /// implemented by the blockchains, which distinguishes “send” and “receive” 
    /// transactions and “receive” transaction requires the same private key as the “send”.
    /// </remarks>
    [PublicAPI]
    public class BuildSingleReceiveTransactionRequest
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [JsonProperty("operationId")]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Hash of the “send” transaction
        /// </summary>
        [JsonProperty("sendTransactionHash")]
        public string SendTransactionHash { get; set; }
    }
}