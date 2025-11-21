namespace WordMaster.Domain.Entities;

public class LogHistory
{
    public long Id { get; set; }
    public Guid? AppUserId { get; set; }
    public string? Email { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccessful { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
    public AppUser? AppUser { get; set; }
}

