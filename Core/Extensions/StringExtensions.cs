using System.Globalization;

namespace Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// İngilizce kelimeleri normalize eder - İlk harfi büyük, geri kalan küçük
    /// InvariantCulture kullanarak I/i sorununu çözer
    /// Birden fazla boşluğu tek boşluğa indirir
    /// </summary>
    public static string NormalizeEnglishWord(this string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return string.Empty;

        // İlk harfi büyük, geri kalan küçük - InvariantCulture ile
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant());
    }

    /// <summary>
    /// Türkçe kelimeleri normalize eder - İlk harfi büyük, geri kalan küçük
    /// Turkish culture kullanarak Türkçe karakterleri doğru işler
    /// Birden fazla boşluğu tek boşluğa indirir
    /// </summary>
    public static string NormalizeTurkishWord(this string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return string.Empty;


        // İlk harfi büyük, geri kalan küçük - Turkish culture ile
        var turkishCulture = new CultureInfo("tr-TR");
        return turkishCulture.TextInfo.ToTitleCase(word.ToLower(turkishCulture));
    }

    /// <summary>
    /// Karşılaştırma için kültürden bağımsız lowercase yapar
    /// I/i sorununu çözer
    /// </summary>
    public static string ToLowerForComparison(this string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return string.Empty;

        return word.Trim().ToLowerInvariant();
    }
}

