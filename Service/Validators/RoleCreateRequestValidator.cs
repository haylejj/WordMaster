using Core.Requests;
using FluentValidation;

namespace Service.Validators;

public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
{
    public RoleCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rol ismi boş bırakılamaz");
    }
}

