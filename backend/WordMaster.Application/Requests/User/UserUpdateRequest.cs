using WordMaster.Domain.Entities;

namespace WordMaster.Application.Requests.User;

public class UserUpdateRequest
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public Gender? Gender { get; set; }
}
