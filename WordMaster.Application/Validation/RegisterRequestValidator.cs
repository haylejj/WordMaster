using FluentValidation;
using WordMaster.Application.Requests;

namespace WordMaster.Application.Validation;

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
    }
}

