using System.Security.Claims;

namespace WordMaster.WebUI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out Guid result) ? result : Guid.Empty;
    }
}

