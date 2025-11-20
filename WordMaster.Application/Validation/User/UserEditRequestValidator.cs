using FluentValidation;
using WordMaster.Application.Requests.User;

namespace WordMaster.Application.Validation.User;

public class UserEditRequestValidator : AbstractValidator<UserEditRequest>
{
    public UserEditRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı Adı alanı boş bırakılamaz");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı boş bırakılamaz")
            .EmailAddress().WithMessage("Lütfen geçerli bir email giriniz.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon alanı boş bırakılamaz");

        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Cinsiyet alanı boş bırakılamaz");

    }
}

