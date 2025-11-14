using System.Globalization;

namespace WordMaster.Domain.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// İngilizce kelimeleri normalize eder - tüm harfleri küçük yapar
    /// InvariantCulture kullanarak I/i sorununu çözer
    /// </summary>
    public static string NormalizeEnglishWord(this string? word)
    {
        return string.IsNullOrWhiteSpace(word) ? string.Empty : word.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Türkçe kelimeleri normalize eder - tüm harfleri küçük yapar
    /// Turkish culture kullanarak Türkçe karakterleri doğru işler
    /// </summary>
    public static string NormalizeTurkishWord(this string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return string.Empty;

        CultureInfo turkishCulture = new("tr-TR");
        return word.Trim().ToLower(turkishCulture);
    }
}

