namespace WordMaster.Application.Constants;

/// <summary>
/// Merkezi cache süreleri sabitleri.
/// Tüm cache işlemlerinde tutarlılık sağlamak için buradan kullanılır.
/// </summary>
public static class CacheDurations
{
    /// <summary>
    /// Practice modu için kısa süreli cache (5 dakika).
    /// Kelime listeleri sık değişmez ama practice sırasında güncel olmalı.
    /// </summary>
    public static readonly TimeSpan Practice = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Normal CRUD işlemleri için standart cache süresi (10 dakika).
    /// </summary>
    public static readonly TimeSpan Normal = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Dashboard istatistikleri için kısa cache süresi (1 dakika).
    /// Kullanıcı güncel veri bekler, practice sonrası invalidate edilir.
    /// </summary>
    public static readonly TimeSpan Statistics = TimeSpan.FromMinutes(1);
}
