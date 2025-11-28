using FluentValidation;
using WordMaster.Application.Requests.User;

namespace WordMaster.Application.Validation.User;

public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı Adı alanı boş bırakılamaz")
            .MaximumLength(256).WithMessage("Kullanıcı adı en fazla 256 karakter olabilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı boş bırakılamaz")
            .EmailAddress().WithMessage("Lütfen geçerli bir email giriniz.")
            .MaximumLength(256).WithMessage("Email en fazla 256 karakter olabilir");

        RuleFor(x => x.Phone)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Telefon numarası en fazla 20 karakter olabilir");

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Now).When(x => x.BirthDate.HasValue)
            .WithMessage("Doğum tarihi bugünden ileri bir tarih olamaz");
    }
}

