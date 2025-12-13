using Microsoft.AspNetCore.Identity;

namespace WordMaster.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public DateTime? BirthDate { get; set; }
    public Gender? Gender { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int CurrentStreak { get; set; } = 0;
    public DateTime? LastStreakUpdateDate { get; set; }
    public ICollection<PracticeHistory> PracticeHistories { get; set; } = [];
}
