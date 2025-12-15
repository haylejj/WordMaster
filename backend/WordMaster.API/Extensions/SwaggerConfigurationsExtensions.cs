using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace WordMaster.API.Extensions;

/// <summary>
/// Swagger/OpenAPI dokümantasyon yapılandırma extension metodlarını içerir.
/// JWT Bearer authentication ve API versiyonlama desteği ile Swagger UI'ı yapılandırır.
/// </summary>
public static class SwaggerConfigurationsExtensions
{
    /// <summary>
    /// Swagger/OpenAPI dokümantasyon yapılandırmasını ekler.
    /// API endpoint'lerini test etmek ve dokümante etmek için Swagger UI kullanılır.
    /// JWT Bearer token authentication desteği ve API versiyonlama ile birlikte yapılandırılır.
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddSwaggerConfigurations(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        // Swagger versiyon konfigürasyonunu DI'a ekle
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
            // XML yorumlarını ekle
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // JWT Bearer authentication tanımı
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header. Örnek: 'Bearer {token}'"
            });

            // Bu ayar, Swagger'ın tanımlanan "Bearer" güvenlik şemasını tüm endpoint'lere otomatik olarak uygulamasını sağlar.
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
