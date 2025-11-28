using WordMaster.Domain.Entities;

namespace WordMaster.Application.Responses.User;

public class UserDetailResponse
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public Gender? Gender { get; set; }

    // Login Statistics
    public int TotalLoginAttempts { get; set; }
    public int SuccessfulLogins { get; set; }
    public int FailedLogins { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIpAddress { get; set; }

    // Word Statistics
    public int WordCount { get; set; }
    public int FavoriteCount { get; set; }
    public int UnknowsCount { get; set; }
    public DateTime? LastPracticeDate { get; set; }
}
