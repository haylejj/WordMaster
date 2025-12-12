using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WordMaster.Application.Constants;

namespace WordMaster.API.Extensions;

/// <summary>
/// Swagger için API versiyonlama yapılandırmasını sağlar.
/// Her API versiyonu için ayrı bir Swagger dökümanı oluşturur.
/// </summary>
/// <remarks>
/// ConfigureSwaggerOptions constructor.
/// </remarks>
/// <param name="provider">API versiyon açıklama sağlayıcısı.</param>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Swagger seçeneklerini yapılandırır.
    /// </summary>
    /// <param name="options">Swagger yapılandırma seçenekleri.</param>
    public void Configure(SwaggerGenOptions options)
    {
        // Her API versiyonu için bir Swagger dökümanı oluştur
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        OpenApiInfo info = new()
        {
            Title = AppInfo.ApiTitle,
            Version = description.ApiVersion.ToString(),
            Description = $"{AppInfo.ApiDescription}<br/><br/>" +
                          $"<strong>Yazılım Sürümü:</strong> {AppInfo.Version}",
            Contact = new OpenApiContact
            {
                Name = "WordMaster Team"
            }
        };

        // Deprecated versiyonlar için uyarı mesajı ekle
        if (description.IsDeprecated)
        {
            info.Description += "<br/><br/><strong style='color:red'>⚠️ Bu API versiyonu kullanımdan kaldırılmıştır (deprecated).</strong>";
        }

        return info;
    }
}

