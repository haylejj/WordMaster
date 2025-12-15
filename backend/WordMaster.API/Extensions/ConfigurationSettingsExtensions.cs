using WordMaster.Domain.Configuration;

namespace WordMaster.API.Extensions;

/// <summary>
/// Uygulama ayarları (appsettings.json) yapılandırma extension metodlarını içerir.
/// Options pattern ile strongly-typed ayarların DI'a kaydedilmesini sağlar.
/// </summary>
public static class ConfigurationSettingsExtensions
{
    /// <summary>
    /// AppSettings.json'daki ayarları servis koleksiyonuna ekleyerek Options pattern'ini kullanır.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<MailHogSettings>(configuration.GetSection("MailHog"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<UrlsSettings>(configuration.GetSection("URLs"));
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));
    }
}
