namespace WordMaster.Application.Responses.User;

public class LastLoginInfoResponse
{
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIpAddress { get; set; }
}
