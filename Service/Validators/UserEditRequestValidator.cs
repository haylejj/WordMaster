using Core.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Service.Validators;

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

        RuleFor(x => x.Picture)
            .Must(BeValidImageFile).When(x => x.Picture != null)
            .WithMessage("Geçerli bir resim dosyası seçiniz (JPG, PNG, GIF)");
    }

    private bool BeValidImageFile(IFormFile? file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}

