namespace WordMaster.Application.Responses.User;

public class UserProfileResponse
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? Gender { get; set; }
}
