using Asp.Versioning;

namespace WordMaster.API.Extensions;

/// <summary>
/// API versiyonlama yapılandırma extension metodlarını içerir.
/// URL path versioning stratejisi ile API versiyonlarını yönetir.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// API Versioning yapılandırmasını ekler.
    /// URL path versioning kullanılır (örn: /api/v1/words)
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddApiVersioningConfigurations(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Varsayılan API versiyonu (versiyon belirtilmezse bu kullanılır)
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // Versiyon belirtilmezse varsayılan versiyonu kullan
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Response header'larında desteklenen ve deprecated versiyonları göster
            // api-supported-versions: 1.0, 2.0
            // api-deprecated-versions: 1.0
            options.ReportApiVersions = true;

            // Versiyon okuma stratejileri (URL path birincil)
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),           // /api/v1/words (birincil)
                new HeaderApiVersionReader("x-api-version") // Header ile de destekle (opsiyonel)
            );
        })
        .AddApiExplorer(options =>
        {
            // Swagger'da versiyon grupları için format: 'v'major[.minor][-status]
            // Örn: v1, v1.0, v2.0-beta
            options.GroupNameFormat = "'v'VVV";

            // Route'lardaki {version:apiVersion} placeholder'ını değiştir
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
