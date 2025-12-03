using Serilog;

namespace WordMaster.API.Extensions;

/// <summary>
/// Serilog konfigürasyonlarını içeren extension sınıfı.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Host builder üzerine Serilog yapılandırmasını ekler.
    /// </summary>
    /// <param name="host">IHostBuilder nesnesi.</param>
    public static void AddSerilogConfigurations(this IHostBuilder host)
    {
        host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration) // Konfigürasyonu appsettings.json dosyasından okur.
                .ReadFrom.Services(services); // DI container'daki servisleri (IHttpContextAccessor vb.) kullanabilmesini sağlar.
                                              // Eski konfigürasyon (Referans için tutulmuştur):
                                              // .MinimumLevel.Information()
                                              // .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                                              // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                              // .MinimumLevel.Override("System", LogEventLevel.Warning)
                                              // .MinimumLevel.Override("Microsoft.AspNetCore.Watch.BrowserRefresh", LogEventLevel.Warning)
                                              // .Enrich.FromLogContext() // Log context'inden (örneğin HTTP request scope'undaki veriler) özellikler ekler.
                                              // .Enrich.WithExceptionDetails() // Hata detaylarını (Exception) zenginleştirilmiş bir formatta loga ekler.
                                              // .Enrich.WithClientIp() // İsteği yapan istemcinin IP adresini loga ekler.
                                              // .Enrich.WithMachineName() // Uygulamanın çalıştığı makine adını loga ekler.
                                              // .Enrich.WithEnvironmentUserName() // Uygulamanın çalıştığı ortamdaki kullanıcı adını loga ekler.
                                              // .Enrich.WithCorrelationId() // İstekleri takip etmek için benzersiz bir Correlation ID ekler.
                                              // .WriteTo.Console(
                                              //     outputTemplate:
                                              //         "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] " +
                                              //         "{Message:lj} {Properties}{NewLine}{Exception}"
                                              // );                              
        });
    }
}
