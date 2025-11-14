namespace WordMaster.Application.ViewModels.AllowedIpAddress;

public class AllowedIpAddressViewModel
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

