namespace WordMaster.Application.Requests;

public class AllowedIpAddressUpdateRequest
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

