using FluentValidation;
using WordMaster.Application.Requests.Role;

namespace WordMaster.Application.Validation.Role;

public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
{
    public RoleCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rol ismi boş bırakılamaz")
            .MinimumLength(2).WithMessage("Rol ismi en az 2 karakter olmalıdır")
            .MaximumLength(50).WithMessage("Rol ismi en fazla 50 karakter olabilir")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Rol ismi sadece harf, rakam ve alt çizgi içerebilir");
    }
}

