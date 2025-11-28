using FluentValidation;
using WordMaster.Application.Requests.Auth;

namespace WordMaster.Application.Validation.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı Adı alanı boş bırakılamaz");

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

        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Cinsiyet alanı boş olamaz.")
            .IsInEnum().WithMessage("Geçersiz cinsiyet değeri.");

    }
}

