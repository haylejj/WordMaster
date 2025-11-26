namespace WordMaster.Application.Responses.AllowedIpAddress;

public class AllowedIpAddressResponse
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
