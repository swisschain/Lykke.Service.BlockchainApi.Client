namespace Lykke.Service.BlockchainApi.Sdk
{
    public interface IAsset
    {
        string AssetId  { get; }
        string Address  { get; }
        string Name     { get; }
        int    Accuracy { get; }

        decimal FromBaseUnit(long amount);
        long ToBaseUnit(decimal amount);
    }
}