using FluentValidation;
using WordMaster.Application.Requests.Permission;

namespace WordMaster.Application.Validation.Permission;

public class UpdateRolePermissionsRequestValidator : AbstractValidator<UpdateRolePermissionsRequest>
{
    public UpdateRolePermissionsRequestValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Rol ID boş olamaz.");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("İzin listesi null olamaz.");
    }
}
