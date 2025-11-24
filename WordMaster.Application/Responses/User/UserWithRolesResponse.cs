using System.Collections.Generic;

namespace WordMaster.Application.Responses.User;

public class UserWithRolesResponse
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? SecurityStamp { get; set; }
    public List<string> Roles { get; set; } = [];
}
