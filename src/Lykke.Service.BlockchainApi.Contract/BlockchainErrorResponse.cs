using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.BlockchainApi.Contract
{
    /// <summary>
    /// Generic error response for blockchain API
    /// </summary>
    [PublicAPI]
    public class BlockchainErrorResponse : ErrorResponse
    {
        /// <summary>
        /// Error code
        /// </summary>
        [JsonProperty("errorCode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public BlockchainErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Creates error response with unknown generic error code
        /// </summary>
        public static BlockchainErrorResponse FromUnknownError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message can't be empty", nameof(message));
            }

            return new BlockchainErrorResponse
            {
                ErrorCode = BlockchainErrorCode.Unknown,
                ErrorMessage = message,
                ModelErrors = new Dictionary<string, List<string>>()
            };
        }

        /// <summary>
        /// Creates error response with well-known transaction execution error code
        /// </summary>
        public static BlockchainErrorResponse FromKnownError(BlockchainErrorCode code)
        {
            string GetMessage()
            {
                switch (code)
                {
                    case BlockchainErrorCode.Unknown:
                        throw new ArgumentOutOfRangeException(nameof(code), code, "Error code should be well-known");

                    case BlockchainErrorCode.AmountIsTooSmall:
                        return "Amount is to small to execute transaction";

                    case BlockchainErrorCode.NotEnoughtBalance:
                        return "Not enought balance on the source address to execute transaction";

                    default:
                        throw new ArgumentOutOfRangeException(nameof(code), code, null);
                }
            }

            return new BlockchainErrorResponse
            {
                ErrorCode = code,
                ErrorMessage = GetMessage(),
                ModelErrors = new Dictionary<string, List<string>>()
            };
        }
    }
}
