using FluentValidation;
using WordMaster.Application.Requests.Auth;

namespace WordMaster.Application.Validation.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı boş bırakılamaz")
            .EmailAddress().WithMessage("Lütfen geçerli bir email giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı boş bırakılamaz");
    }
}

