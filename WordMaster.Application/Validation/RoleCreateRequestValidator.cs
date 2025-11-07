using FluentValidation;

using WordMaster.Application.Requests;
namespace WordMaster.Application.Validation;

public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
{
    public RoleCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rol ismi boş bırakılamaz");
    }
}

