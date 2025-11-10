using WordMaster.Domain.Entities;

namespace WordMaster.Application.Requests;

public class UserUpdateRequest
{
    public string Id { get; set; } = null!;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? City { get; set; }
    public Gender? Gender { get; set; }
}
