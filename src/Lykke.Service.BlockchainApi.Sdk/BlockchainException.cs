using System;
using Lykke.Service.BlockchainApi.Contract;

namespace Lykke.Service.BlockchainApi.Sdk
{
    public class BlockchainException : Exception
    {
        public BlockchainException(BlockchainErrorCode errorCode, string message) : base(message) => ErrorCode = errorCode;

        public BlockchainErrorCode ErrorCode { get; }
    }
}