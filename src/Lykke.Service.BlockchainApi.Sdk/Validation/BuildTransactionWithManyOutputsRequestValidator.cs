using FluentValidation;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public class BuildTransactionWithManyOutputsRequestValidator : AbstractValidator<BuildTransactionWithManyOutputsRequest>
    {
        public BuildTransactionWithManyOutputsRequestValidator()
        {
            RuleFor(r => r.AssetId)
                .NotEmpty();

            RuleFor(r => r.FromAddress)
                .NotEmpty();

            RuleFor(r => r.OperationId)
                .NotEmpty();

            RuleFor(r => r.Outputs)
                .NotNull()
                .Must(x => x.Count > 0);

            RuleForEach(r => r.Outputs)
                .SetValidator(new BuildingTransactionOutputContractValidator());
        }

        public class BuildingTransactionOutputContractValidator : AbstractValidator<BuildingTransactionOutputContract>
        {
            public BuildingTransactionOutputContractValidator()
            {
                RuleFor(x => x.Amount)
                    .NotEmpty();

                RuleFor(x => x.ToAddress)
                    .NotEmpty();
            }
        }
    }
}