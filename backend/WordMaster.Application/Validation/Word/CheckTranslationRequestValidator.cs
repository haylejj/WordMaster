using FluentValidation;
using WordMaster.Application.Requests.Word;

namespace WordMaster.Application.Validation.Word;

public class CheckTranslationRequestValidator : AbstractValidator<CheckTranslationRequest>
{
    public CheckTranslationRequestValidator()
    {
        RuleFor(x => x.WordId)
        .GreaterThan(0).WithMessage("Geçerli bir kelime ID'si giriniz.");

        RuleFor(x => x.Answer)
        .NotEmpty().WithMessage("Cevap boş olamaz.")
        .MaximumLength(100).WithMessage("Cevap 100 karakterden uzun olamaz.");
    }
}

