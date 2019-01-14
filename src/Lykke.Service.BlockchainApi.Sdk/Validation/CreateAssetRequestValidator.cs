using FluentValidation;
using Lykke.Service.BlockchainApi.Sdk.Models;

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
                .NotEmpty();
        }
    }
}