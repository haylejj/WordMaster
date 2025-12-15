using FluentValidation;
using WordMaster.Application.Requests.Auth;

namespace WordMaster.Application.Validation.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş bırakılamaz")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş bırakılamaz")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı boş bırakılamaz")
            .EmailAddress().WithMessage("Lütfen geçerli bir email giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı boş bırakılamaz")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır");

        RuleFor(x => x.PasswordConfirm)
            .NotEmpty().WithMessage("Şifre tekrar alanı boş bırakılamaz")
            .Equal(x => x.Password).WithMessage("Şifreler aynı değildir.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon alanı boş bırakılamaz");
    }
}

