using FluentValidation;
using WordMaster.Application.ViewModels.Auth;

namespace WordMaster.Application.Validation.Auth;

public class ForgetPasswordViewModelValidator : AbstractValidator<ForgetPasswordViewModel>
{
    public ForgetPasswordViewModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı boş bırakılamaz.")
            .EmailAddress().WithMessage("Lütfen geçerli bir email adresi giriniz.");
    }
}

