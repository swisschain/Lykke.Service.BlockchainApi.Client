using FluentValidation;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public class BuildTransactionWithManyInputsRequestValidator : AbstractValidator<BuildTransactionWithManyInputsRequest>
    {
        public BuildTransactionWithManyInputsRequestValidator()
        {
            RuleFor(r => r.AssetId)
                .NotEmpty();

            RuleFor(r => r.OperationId)
                .NotEmpty();

            RuleFor(r => r.ToAddress)
                .NotEmpty();

            RuleFor(r => r.Inputs)
                .NotNull()
                .Must(x => x.Count > 0);

            RuleForEach(r => r.Inputs)
                .SetValidator(new BuildingTransactionInputContractValidator());
        }

        public class BuildingTransactionInputContractValidator : AbstractValidator<BuildingTransactionInputContract>
        {
            public BuildingTransactionInputContractValidator()
            {
                RuleFor(x => x.Amount)
                    .NotEmpty();

                RuleFor(x => x.FromAddress)
                    .NotEmpty();
            }
        }
    }
}