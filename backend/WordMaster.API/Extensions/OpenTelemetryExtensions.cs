using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using WordMaster.Application.Constants;

namespace WordMaster.API.Extensions;

/// <summary>
/// OpenTelemetry  yapılandırması için extension metodları.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// OpenTelemetry metrics servislerini DI container'a ekler ve yapılandırır.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Updated service collection</returns>
    public static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                // AddMeter: OpenTelemetry'ye "bu isimdeki Meter'dan gelen verileri topla" emrini verir.
                // Eğer bunu yazmazsak, kod içinde metric (counter) arttırsak bile buraya gelmez, kaybolur.
                // Sadece abone olduğumuz (AddMeter dediğimiz) metricler toplanır.
                metrics.AddMeter(OpenTelemetryMetric.MeterName);
                // ConfigureResource: Bu verinin "kimden" geldiğini tanımlar (Kimlik Kartı).
                // "WordMaster" servisinden, "1.0.0" versiyonundan geliyor der.
                // ConfigureResource kullanmak, default gelen host/os bilgilerini korur, üzerine ekler.
                metrics.ConfigureResource(resource =>
                {
                    resource.AddService(serviceName: OpenTelemetryMetric.ServiceName, serviceVersion: OpenTelemetryMetric.MeterVersion);
                    resource.AddAttributes(new List<KeyValuePair<string, object>>
                    {
                        new("environment",AppInfo.EnvironmentName)
                    });
                });
                // 2. ASP.NET Core otomatik metrics (request duration, vb.)
                metrics.AddAspNetCoreInstrumentation();

                // 3. Runtime metrics (GC, thread pool, vb.)
                metrics.AddRuntimeInstrumentation();

                // 4. HTTP Client metrics (dışarıya yapılan istekler)
                metrics.AddHttpClientInstrumentation();

                metrics.AddOtlpExporter(options =>
                {
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    options.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                });
            });
        return services;
    }
}
