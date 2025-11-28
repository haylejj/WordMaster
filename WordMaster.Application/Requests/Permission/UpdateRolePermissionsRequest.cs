namespace WordMaster.Application.Requests.Permission;

public class UpdateRolePermissionsRequest
{
    public Guid RoleId { get; set; }
    public required List<Guid> PermissionIds { get; set; }
}
