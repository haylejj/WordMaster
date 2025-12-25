using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

namespace WordMaster.API.Extensions;

/// <summary>
/// Forwarded headers yapılandırması için extension metodları.
/// </summary>
public static class ForwardHeadersConfigurationsExtensions
{
    /// <summary>
    /// Forwarded headers (X-Forwarded-For, X-Forwarded-Proto) yapılandırmasını ekler.
    /// Reverse proxy (Nginx, etc.) arkasında çalışırken gereklidir.
    /// </summary>
    /// <param name="services">Service collection</param>
    public static void AddForwardedHeadersConfigurations(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
    }
}
