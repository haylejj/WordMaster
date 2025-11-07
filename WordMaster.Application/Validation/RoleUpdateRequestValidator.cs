using FluentValidation;
using WordMaster.Application.Requests;

namespace WordMaster.Application.Validation;

public class RoleUpdateRequestValidator : AbstractValidator<RoleUpdateRequest>
{
    public RoleUpdateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rol ID boş bırakılamaz");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rol ismi boş bırakılamaz");
    }
}

