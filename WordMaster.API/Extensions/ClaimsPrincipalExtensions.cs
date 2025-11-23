using System.Security.Claims;

namespace WordMaster.API.Extensions;

/// <summary>
/// ClaimsPrincipal (User) nesnesi üzerinden kullanıcı bilgilerine kolay erişim sağlayan extension metodlar.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Giriş yapmış kullanıcının ID'sini döner.
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Giriş yapmış kullanıcının kullanıcı adını döner.
    /// </summary>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Giriş yapmış kullanıcının email adresini döner.
    /// </summary>
    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }
}
