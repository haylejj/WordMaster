namespace WordMaster.Application.Requests.Auth;

/// <summary>
/// Token yenileme isteği.
/// Refresh token artık HttpOnly cookie'den okunduğu için sadece access token gerekli.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Süresi dolmuş veya dolmak üzere olan access token.
    /// Bu token'dan kullanıcı bilgileri çıkarılır.
    /// </summary>
    public string AccessToken { get; set; } = null!;
}
