using FluentValidation;
using Lykke.Service.BlockchainApi.Contract.Testing;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public class TestingTransferRequestValidator : AbstractValidator<TestingTransferRequest>
    {
        public TestingTransferRequestValidator()
        {
            RuleFor(r => r.Amount)
                .NotEmpty();

            RuleFor(r => r.AssetId)
                .NotEmpty();

            RuleFor(r => r.FromAddress)
                .NotEmpty();

            RuleFor(r => r.FromPrivateKey)
                .NotEmpty();

            RuleFor(r => r.ToAddress)
                .NotEmpty();
        }
    }
}