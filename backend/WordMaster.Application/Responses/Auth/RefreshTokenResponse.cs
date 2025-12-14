namespace WordMaster.Application.Responses.Auth;

/// <summary>
/// Token yenileme işlemi sonucu dönen response modeli.
/// Yeni RefreshToken HttpOnly cookie ile gönderilir, body'de sadece AccessToken döner.
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// Yeni oluşturulan JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Access token'ın süresinin dolacağı tarih (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
