using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WordMaster.Domain.Configuration;

namespace WordMaster.Infrastructure.Helpers;

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

/// <summary>
/// Refresh token cookie işlemlerini yöneten helper sınıfı implementasyonu.
/// Development ve Production ortamlarına göre farklı güvenlik ayarları kullanır.
/// </summary>
public class RefreshTokenCookieHelper(
    IOptions<JwtSettings> jwtSettings,
    IHostEnvironment hostEnvironment) : IRefreshTokenCookieHelper
{
    private const string CookieName = "refreshToken";
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly bool _isDevelopment = hostEnvironment.IsDevelopment();

    /// <summary>
    /// Refresh token'ı HttpOnly cookie olarak set eder.
    /// </summary>
    public void SetRefreshTokenCookie(HttpContext context, string refreshToken)
    {
        CookieOptions cookieOptions = new()
        {
            HttpOnly = true,                                    // JavaScript erişemez (XSS koruması)
            Secure = !_isDevelopment,
            SameSite = _isDevelopment
                ? SameSiteMode.Lax                              // Development: http/https cross-scheme için
                : SameSiteMode.Strict,                          // Production: Maksimum CSRF koruması
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays),
            Path = "/api",                                      // Sadece API endpoint'lerine gönderilir
            IsEssential = true                                  // GDPR: Gereklidir işaretlenmesi
        };

        context.Response.Cookies.Append(CookieName, refreshToken, cookieOptions);
    }

    /// <summary>
    /// Request'ten refresh token cookie'sini okur.
    /// </summary>
    public string? GetRefreshTokenFromCookie(HttpContext context)
    {
        return context.Request.Cookies[CookieName];
    }

    /// <summary>
    /// Refresh token cookie'sini siler (logout için).
    /// </summary>
    public void DeleteRefreshTokenCookie(HttpContext context)
    {
        CookieOptions cookieOptions = new()
        {
            HttpOnly = true,
            Secure = !_isDevelopment,
            SameSite = _isDevelopment ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/api"
        };

        context.Response.Cookies.Delete(CookieName, cookieOptions);
    }
}
