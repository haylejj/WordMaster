namespace WordMaster.Application.Responses;

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
