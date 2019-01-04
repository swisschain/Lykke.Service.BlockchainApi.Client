namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Represents configured asset for integration.
    /// </summary>
    public interface IAsset
    {
        string AssetId  { get; }
        string Address  { get; }
        string Name     { get; }
        int    Accuracy { get; }

        /// <summary>
        /// Converts amount in base units to decimal amount with asset accuracy, i.e. 12345000 with accuracy 6 => 12.345
        /// </summary>
        /// <param name="amount">Amount in base units</param>
        /// <returns>Decimal amount</returns>
        decimal FromBaseUnit(long amount);

        /// <summary>
        /// Converts decimal amount to base units with asset accuracy, i.e. 12.345 with accuracy 6 => 12345000.
        /// </summary>
        /// <param name="amount">Decimal amount</param>
        /// <returns>Amount in base units</returns>
        long ToBaseUnit(decimal amount);
    }
}