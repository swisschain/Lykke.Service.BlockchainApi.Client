namespace Lykke.Service.BlockchainApi.Sdk
{
    public class BlockchainBalance
    {
        public BlockchainBalance() {}
        public BlockchainBalance(string address, string assetId, decimal amount) =>
            (Address, AssetId, Amount) = (address, assetId, amount);

        public string  Address { get; set; }
        public string  AssetId { get; set; }
        public decimal Amount  { get; set; }
    }
}