namespace WordMaster.Application.Responses.Auth;

/// <summary>
/// Token yenileme işlemi sonucu JwtService'den dönen internal response.
/// Controller bu bilgiyi kullanarak:
/// - Access token'ı response body'de döner
/// - Refresh token'ı HttpOnly cookie olarak set eder
/// </summary>
public class RefreshTokenInternalResponse
{
    /// <summary>
    /// Yeni oluşturulan JWT access token (response body'de dönecek)
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Yeni oluşturulan Refresh Token (HttpOnly cookie olarak set edilecek)
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Access token'ın süresinin dolacağı tarih (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
