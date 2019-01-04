namespace Lykke.Service.BlockchainApi.Sdk
{
    /// <summary>
    /// Represents a particular transfer (outcome + income) within operation.
    /// </summary>
    public interface IOperationAction
    {
        string  From        { get; }
        string  FromContext { get; }
        string  To          { get; }
        decimal Amount      { get; }
    }
}