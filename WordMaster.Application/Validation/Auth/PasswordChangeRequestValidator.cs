using FluentValidation;
using WordMaster.Application.Requests.Auth;
namespace WordMaster.Application.Validation.Auth;

public class PasswordChangeRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public PasswordChangeRequestValidator()
    {
        RuleFor(x => x.PasswordOld)
            .NotEmpty().WithMessage("Eski Şifre alanı boş bırakılamaz");

        RuleFor(x => x.PasswordNew)
            .NotEmpty().WithMessage("Yeni Şifre alanı boş bırakılamaz")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır");

        RuleFor(x => x.PasswordConfirm)
            .NotEmpty().WithMessage("Yeni Şifre tekrar alanı boş bırakılamaz")
            .Equal(x => x.PasswordNew).WithMessage("Şifreler aynı değildir.");
    }
}

