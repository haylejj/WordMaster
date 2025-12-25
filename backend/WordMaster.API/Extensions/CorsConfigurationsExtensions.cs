using Microsoft.EntityFrameworkCore;

namespace WordMaster.API.Extensions;

/// <summary>
/// CORS (Cross-Origin Resource Sharing) yapılandırması için extension metodları.
/// </summary>
public static class CorsConfigurationsExtensions
{
    /// <summary>
    /// CORS politikasını yapılandırır. Frontend uygulamasının API'ye erişimini sağlar.
    /// </summary>
    /// <param name="services">Service collection</param>
    public static void AddCorsConfigurations(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "https://localhost:5173") // Frontend URL
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
        });
    }
}
