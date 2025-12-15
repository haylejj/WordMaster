using Microsoft.AspNetCore.Http;

namespace WordMaster.Application.Helpers;

/// <summary>
/// Refresh token cookie işlemlerini yöneten helper sınıfı.
/// HttpOnly, Secure ve SameSite cookie ile güvenlik sağlar.
/// </summary>
public interface IRefreshTokenCookieHelper
{
    /// <summary>
    /// Refresh token'ı HttpOnly cookie olarak set eder.
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="refreshToken">Plain text refresh token</param>
    void SetRefreshTokenCookie(HttpContext context, string refreshToken);

    /// <summary>
    /// Request'ten refresh token cookie'sini okur.
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Refresh token veya null</returns>
    string? GetRefreshTokenFromCookie(HttpContext context);

    /// <summary>
    /// Refresh token cookie'sini siler (logout için).
    /// </summary>
    /// <param name="context">HTTP context</param>
    void DeleteRefreshTokenCookie(HttpContext context);
}
