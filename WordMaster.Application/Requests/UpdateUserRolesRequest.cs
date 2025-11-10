namespace WordMaster.Application.Requests;

public class UpdateUserRolesRequest
{
    public string UserId { get; set; } = null!;
    public List<RoleAssignmentRequest> Roles { get; set; } = new();
}

public class RoleAssignmentRequest
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Exist { get; set; }
}

