using FluentValidation;
using WordMaster.Application.Requests.Unknows;

namespace WordMaster.Application.Validation.Unknows;

public class ToggleUnknowsRequestValidator : AbstractValidator<ToggleUnknowsRequest>
{
    public ToggleUnknowsRequestValidator()
    {
        RuleFor(x => x.WordId)
            .GreaterThan(0).WithMessage("Geçersiz kelime ID'si.");
    }
}
