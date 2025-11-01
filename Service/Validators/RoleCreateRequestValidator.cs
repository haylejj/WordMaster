using FluentValidation;
using Core.Requests;

namespace UserInterface.Validators
{
    public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
    {
        public RoleCreateRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Rol ismi boş bırakılamaz");
        }
    }
}

