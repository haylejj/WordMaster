using Core.Dto;
using FluentValidation;

namespace Service.Validators;

public class WordDtoValidator : AbstractValidator<WordDto>
{
    public WordDtoValidator()
    {
        RuleFor(x => x.EnglishWord)
            .NotEmpty().WithMessage("İngilizce kelime alanı boş bırakılamaz")
            .MaximumLength(30).WithMessage("İngilizce kelime en fazla 30 karakter olabilir")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("İngilizce kelime boş olamaz");

        RuleFor(x => x.TurkishWord)
            .NotEmpty().WithMessage("Türkçe kelime alanı boş bırakılamaz")
            .MaximumLength(60).WithMessage("Türkçe kelime en fazla 60 karakter olabilir")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Türkçe kelime boş olamaz");
    }
}

