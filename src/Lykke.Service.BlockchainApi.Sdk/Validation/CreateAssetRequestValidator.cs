using FluentValidation;
using Lykke.Service.BlockchainApi.Sdk.Models;
using static Lykke.Service.BlockchainApi.Sdk.Validation.Validators;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public class CreateAssetRequestValidator : AbstractValidator<CreateAssetRequest>
    {
        public CreateAssetRequestValidator()
        {
            RuleFor(r => r.Accuracy)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(28);

            RuleFor(r => r.AssetId)
                .NotEmpty()
                .Must(ValidateAzureKey).WithMessage(MustNotContainInvalidAzureSymbols);
        }
    }
}