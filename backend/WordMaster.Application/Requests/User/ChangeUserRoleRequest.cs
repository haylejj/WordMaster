namespace WordMaster.Application.Requests.User;

public class ChangeUserRoleRequest
{
    public string UserId { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
