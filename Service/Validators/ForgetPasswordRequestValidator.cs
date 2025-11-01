using FluentValidation;
using Core.Requests;

namespace UserInterface.Validators
{
    public class ForgetPasswordRequestValidator : AbstractValidator<ForgetPasswordRequest>
    {
        public ForgetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email alanı boş bırakılamaz")
                .EmailAddress().WithMessage("Lütfen geçerli bir email giriniz.");
        }
    }
}

