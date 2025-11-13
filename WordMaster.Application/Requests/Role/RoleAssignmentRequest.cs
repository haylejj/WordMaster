namespace WordMaster.Application.Requests.Role;

public class RoleAssignmentRequest
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Exist { get; set; }
}

