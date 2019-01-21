using FluentValidation;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using static Lykke.Service.BlockchainApi.Sdk.Validation.Validators;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public class BroadcastTransactionRequestValidator : AbstractValidator<BroadcastTransactionRequest>
    {
        public BroadcastTransactionRequestValidator()
        {
            RuleFor(r => r.OperationId)
                .NotEmpty();

            RuleFor(r => r.SignedTransaction)
                .NotEmpty()
                .Must(ValidateBase64).WithMessage(MustBeBase64Encoded);
        }
    }
}