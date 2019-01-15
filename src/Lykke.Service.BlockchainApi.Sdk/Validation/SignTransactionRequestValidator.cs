using FluentValidation;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using static Lykke.Service.BlockchainApi.Sdk.Validation.Validators;

namespace Lykke.Service.BlockchainApi.Sdk.Validation
{
    public class SignTransactionRequestValidator : AbstractValidator<SignTransactionRequest>
    {
        public SignTransactionRequestValidator()
        {
            RuleFor(r => r.PrivateKeys)
                .NotNull()
                .Must(x => x.Count > 0);

            RuleFor(r => r.TransactionContext)
                .NotEmpty()
                .Must(ValidateBase64).WithMessage(MustBeBase64Encoded);
        }
    }
}