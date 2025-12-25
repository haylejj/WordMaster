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

    // ═══════════════════════════════════════════════════════════════════════════
    // KELIME METRİKLERİ
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Toplam oluşturulan kelime sayısı (Counter - Sadece artar)
    /// </summary>
    public static Counter<long> WordsCreated = Meter.CreateCounter<long>(
        name: "wordmaster.words.created_total",
        unit: "words",
        description: "Total number of words created"
    );

    /// <summary>
    /// Toplam silinen kelime sayısı (Counter - Sadece artar)
    /// </summary>
    public static Counter<long> WordsDeleted = Meter.CreateCounter<long>(
        name: "wordmaster.words.deleted_total",
        unit: "words",
        description: "Total number of words deleted"
    );

    // ═══════════════════════════════════════════════════════════════════════════
    // PRATİK METRİKLERİ
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Başlatılan pratik sayısı (Counter)
    /// </summary>
    public static Counter<long> PracticeSessions = Meter.CreateCounter<long>(
        name: "wordmaster.practice.sessions_total",
        unit: "sessions",
        description: "Total practice sessions started"
    );

    /// <summary>
    /// Pratik süresi dağılımı (Histogram - Percentile hesaplanabilir)
    /// </summary>
    public static Histogram<double> PracticeDuration = Meter.CreateHistogram<double>(
        name: "wordmaster.practice.duration_seconds",
        unit: "seconds",
        description: "Duration of practice sessions in seconds"
    );

    // ═══════════════════════════════════════════════════════════════════════════
    // KULLANICI METRİKLERİ
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Kayıt olan kullanıcı sayısı (Counter)
    /// </summary>
    public static Counter<long> UsersRegistered = Meter.CreateCounter<long>(
        name: "wordmaster.users.registered_total",
        unit: "users",
        description: "Total number of registered users"
    );

    /// <summary>
    /// Login denemeleri (Counter - Labels: status=success/failed)
    /// </summary>
    public static Counter<long> LoginAttempts = Meter.CreateCounter<long>(
        name: "wordmaster.auth.login_attempts_total",
        unit: "attempts",
        description: "Total login attempts (use status label for success/failed)"
    );

    // ═══════════════════════════════════════════════════════════════════════════
    // FOLDER METRİKLERİ
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Oluşturulan folder sayısı (Counter)
    /// </summary>
    public static Counter<long> FoldersCreated = Meter.CreateCounter<long>(
        name: "wordmaster.folders.created_total",
        unit: "folders",
        description: "Total number of folders created"
    );

    /// <summary>
    /// Folderlara eklenen kelime sayısı (Counter)
    /// </summary>
    public static Counter<long> WordsAddedToFolders = Meter.CreateCounter<long>(
        name: "wordmaster.folders.words_added_total",
        unit: "words",
        description: "Total number of words added to folders"
    );
}