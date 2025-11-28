namespace WordMaster.Domain.Entities;

public class AllowedIpAddress
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

