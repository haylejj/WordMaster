namespace WordMaster.Application.Requests;

public class UpdateUserRolesRequest
{
    public string UserId { get; set; } = null!;
    public List<RoleAssignmentRequest> Roles { get; set; } = [];
}

