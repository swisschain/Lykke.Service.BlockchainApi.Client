using FluentValidation;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public class BuildSingleTransactionRequestValidator : AbstractValidator<BuildSingleTransactionRequest>
    {
        public BuildSingleTransactionRequestValidator()
        {
            RuleFor(r => r.Amount)
                .NotEmpty();

            RuleFor(r => r.AssetId)
                .NotEmpty();

            RuleFor(r => r.FromAddress)
                .NotEmpty();

            RuleFor(r => r.OperationId)
                .NotEmpty();

            RuleFor(r => r.ToAddress)
                .NotEmpty();
        }
    }
}