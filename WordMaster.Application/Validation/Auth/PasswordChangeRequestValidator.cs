using FluentValidation;
using WordMaster.Application.Requests.Auth;
namespace WordMaster.Application.Validation.Auth;

public class PasswordChangeRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public PasswordChangeRequestValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Eski Şifre alanı boş bırakılamaz");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni Şifre alanı boş bırakılamaz")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Yeni Şifre tekrar alanı boş bırakılamaz")
            .Equal(x => x.NewPassword).WithMessage("Şifreler aynı değildir.");
    }
}

