using System.Text.Json.Serialization;
using WordMaster.Domain.Entities;

namespace WordMaster.Application.Responses.User;

public class UserWithRolesResponse
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    [JsonIgnore]
    public string? SecurityStamp { get; set; }
    public bool IsLockedOut { get; set; }
    public Gender? Gender { get; set; }
    public List<string> Roles { get; set; } = [];
}
