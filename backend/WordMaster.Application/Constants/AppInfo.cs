using System.Reflection;

namespace WordMaster.Application.Constants;

/// <summary>
/// Uygulama bilgilerini içeren sabit değerler.
/// </summary>
public static class AppInfo
{
    /// <summary>
    /// Uygulama adı.
    /// </summary>
    public const string Name = "WordMaster";

    /// <summary>
    /// Yazılım sürümü (.csproj'dan okunur).
    /// </summary>
    public static readonly string Version = Assembly.GetEntryAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        ?? "1.0.0";

    /// <summary>
    /// API başlığı (Swagger için).
    /// </summary>
    public const string ApiTitle = "WordMaster API";

    /// <summary>
    /// API açıklaması.
    /// </summary>
    public const string ApiDescription = "WordMaster - Kelime öğrenme uygulaması API'si";
}

