namespace WordMaster.Application.Requests.AllowedIpAddress;

public class AllowedIpAddressCreateRequest
{
    public string IpAddress { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

