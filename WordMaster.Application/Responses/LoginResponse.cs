namespace WordMaster.Application.Responses;

/// <summary>
/// Login işlemi sonucu dönen response modeli.
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
