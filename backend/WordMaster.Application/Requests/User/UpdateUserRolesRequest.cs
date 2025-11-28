using WordMaster.Application.Requests.Role;

namespace WordMaster.Application.Requests.User;

public class UpdateUserRolesRequest
{
    public string UserId { get; set; } = null!;
    public List<RoleAssignmentRequest> Roles { get; set; } = [];
}

