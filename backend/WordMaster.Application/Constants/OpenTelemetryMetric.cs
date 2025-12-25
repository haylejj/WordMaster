using System.Diagnostics.Metrics;

namespace WordMaster.Application.Constants;

public static class OpenTelemetryMetric
{
    // MeterName: Metric'leri gruplamak için kullanılan benzersiz ad (Namespace gibi).
    // OpenTelemetry bu isme bakarak hangi metric'leri toplayacağını bilir.
    public const string MeterName = "WordMaster";
    // MeterVersion: Metric yapısının versiyonu.
    public const string MeterVersion = "1.0.0";
    public const string ServiceName = "WordMaster";
    public const string ServiceVersion = "1.0.0";
    // Meter: Metric üreticisi (Factory).
    // Uygulama içinde Counter, Histogram gibi ölçümleri bu nesne üzerinden yaratırız.
    // Örn: Meter.CreateCounter("login_count");
    public static Meter Meter = new(MeterName, MeterVersion);
}