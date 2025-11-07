using FluentValidation;

using WordMaster.Application.Requests;
namespace WordMaster.Application.Validation;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı boş bırakılamaz")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır");

        RuleFor(x => x.PasswordConfirm)
            .NotEmpty().WithMessage("Şifre tekrar alanı boş bırakılamaz")
            .Equal(x => x.Password).WithMessage("Şifreler aynı değildir.");
    }
}

