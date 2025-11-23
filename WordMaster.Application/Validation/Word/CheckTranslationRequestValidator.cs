using FluentValidation;
using WordMaster.Application.Requests.Word;

namespace WordMaster.Application.Validation.Word;

public class CheckTranslationRequestValidator : AbstractValidator<CheckTranslationRequest>
{
    public CheckTranslationRequestValidator()
    {
        RuleFor(x => x.EnglishWord)
            .NotEmpty().WithMessage("İngilizce kelime boş olamaz.")
            .MaximumLength(100).WithMessage("İngilizce kelime 100 karakterden uzun olamaz.");

        RuleFor(x => x.TurkishWord)
            .NotEmpty().WithMessage("Türkçe kelime boş olamaz.")
            .MaximumLength(100).WithMessage("Türkçe kelime 100 karakterden uzun olamaz.");
    }
}

