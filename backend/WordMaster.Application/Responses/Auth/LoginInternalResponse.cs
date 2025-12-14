namespace WordMaster.Application.Responses.Auth;

/// <summary>
/// LoginService'den AuthController'a dönen internal response.
/// Hem access token hem de refresh token içerir.
/// Controller bu bilgiyi kullanarak:
/// - Access token'ı response body'de döner
/// - Refresh token'ı HttpOnly cookie olarak set eder
/// </summary>
public class LoginInternalResponse
{
    /// <summary>
    /// JWT Access Token (response body'de dönecek)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh Token (HttpOnly cookie olarak set edilecek)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
