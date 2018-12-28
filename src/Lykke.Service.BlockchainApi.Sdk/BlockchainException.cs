using System;
using Lykke.Service.BlockchainApi.Contract;

namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Well-known blockchain exception.
    /// When broadcasting must be thrown only if blockchain is not affected.
    /// </summary>
    public class BlockchainException : Exception
    {
        public BlockchainException(BlockchainErrorCode errorCode, string message) : base(message) => ErrorCode = errorCode;

        public BlockchainErrorCode ErrorCode { get; }
    }
}