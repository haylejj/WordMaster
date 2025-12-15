namespace WordMaster.Application.Responses.Auth;

/// <summary>
/// Login işlemi sonucu dönen response modeli.
/// RefreshToken artık HttpOnly cookie ile gönderildiği için body'de yer almaz.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Kısa ömürlü JWT access token (15 dakika).
    /// Frontend bu token'ı memory'de tutmalı ve Authorization header'ında göndermelidir.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Access token'ın süresinin dolacağı tarih (UTC).
    /// Frontend bu bilgiyi kullanarak token yenileme zamanlaması yapabilir.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
