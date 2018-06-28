using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Transaction building result
    /// </summary>
    [PublicAPI]
    public enum TransactionBroadcastingResult
    {
        /// <summary>
        /// Transaction is broadcasted successfully
        /// </summary>
        Success,

        /// <summary>
        /// Transaction with specified operation ID is already broadcasted
        /// </summary>
        AlreadyBroadcasted,

        /// <summary>
        /// Amount is too small to execute the transaction
        /// </summary>
        AmountIsTooSmall,

        /// <summary>
        /// Transaction can’t be executed due to balance insufficiency on the source address
        /// </summary>
        NotEnoughBalance,

        /// <summary>
        /// Transaction should be built, signed and broadcasted again
        /// </summary>
        BuildingShouldBeRepeated
    }
}
