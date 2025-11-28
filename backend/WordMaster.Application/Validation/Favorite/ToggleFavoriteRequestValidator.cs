using FluentValidation;
using WordMaster.Application.Requests.Favorite;

namespace WordMaster.Application.Validation.Favorite;

public class ToggleFavoriteRequestValidator : AbstractValidator<ToggleFavoriteRequest>
{
    public ToggleFavoriteRequestValidator()
    {
        RuleFor(x => x.WordId)
            .GreaterThan(0).WithMessage("Geçersiz kelime ID'si.");
    }
}
