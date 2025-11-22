using WordMaster.Domain.Entities;

namespace WordMaster.Application.Responses;

/// <summary>
/// Kullanıcı ve rolleri içeren response modeli.
/// </summary>
public class UserWithRolesResponse
{
    public AppUser User { get; set; } = null!;
    public IList<string> Roles { get; set; } = new List<string>();
}
