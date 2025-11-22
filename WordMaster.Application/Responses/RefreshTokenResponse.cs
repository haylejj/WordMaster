namespace WordMaster.Application.Responses;

/// <summary>
/// JWT token refresh işlemi sonucu dönen response modeli.
/// </summary>
public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
