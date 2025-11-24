using WordMaster.Application.Responses.Role;

namespace WordMaster.Application.Requests.Role;

public class AssignRolesRequest
{
    public string UserId { get; set; } = null!;
    public List<AssignToRoleResponse> Roles { get; set; } = [];
}

