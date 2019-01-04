namespace Lykke.Service.BlockchainApi.Sdk.Domain.Operations
{
    public class OperationAction : IOperationAction
    {
        public OperationAction() { }
        public OperationAction(string actionId, string from, string fromContext, string to, decimal amount) =>
            (ActionId, From, FromContext, To, Amount) = (actionId, from, fromContext, to, amount);

        public string  ActionId    { get; set; }
        public string  From        { get; set; }
        public string  FromContext { get; set; }
        public string  To          { get; set; }
        public decimal Amount      { get; set; }

        public bool IsFake(char separator) => From.Split(separator)[0] == To.Split(separator)[0];
        public bool IsReal(char separator) => !IsFake(separator);
    }
}