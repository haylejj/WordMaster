using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Text;
using WordMaster.Application.Helpers;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.Helpers;

/// <summary>
/// Kullanıcı adı oluşturma işlemleri için helper sınıfı.
/// </summary>
public class UsernameHelper(UserManager<AppUser> userManager) : IUsernameHelper
{
    /// <inheritdoc />
    public async Task<string> GenerateUniqueUsernameAsync(string firstName, string lastName)
    {
        string baseUsername = GenerateUsernameFromName(firstName, lastName);
        return await EnsureUniqueUsernameAsync(baseUsername);
    }

    /// <summary>
    /// FirstName ve LastName'den benzersiz bir username oluşturur.
    /// Örn: "Ahmet", "Yılmaz" -> "ahmetyilmaz"
    /// </summary>
    private static string GenerateUsernameFromName(string firstName, string lastName)
    {
        // Türkçe karakterleri ASCII'ye dönüştür
        string normalizedFirst = RemoveDiacritics(firstName.ToLowerInvariant());
        string normalizedLast = RemoveDiacritics(lastName.ToLowerInvariant());

        // Boşlukları ve özel karakterleri kaldır
        StringBuilder sb = new();
        foreach (char c in normalizedFirst + normalizedLast)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }

        string baseUsername = sb.ToString();
        return string.IsNullOrEmpty(baseUsername) ? "user" : baseUsername;
    }

    /// <summary>
    /// Diacritics (aksanlar) ve Türkçe karakterleri ASCII'ye dönüştürür.
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        // Türkçe karakter dönüşümleri
        text = text.Replace('ı', 'i')
                   .Replace('ğ', 'g')
                   .Replace('ü', 'u')
                   .Replace('ş', 's')
                   .Replace('ö', 'o')
                   .Replace('ç', 'c')
                   .Replace('İ', 'i')
                   .Replace('Ğ', 'g')
                   .Replace('Ü', 'u')
                   .Replace('Ş', 's')
                   .Replace('Ö', 'o')
                   .Replace('Ç', 'c');

        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new();

        foreach (char c in normalizedString)
        {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Username'in benzersiz olmasını sağlar. Eğer kullanılıyorsa sonuna sayı ekler.
    /// </summary>
    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername)
    {
        string username = baseUsername;
        int suffix = 1;

        // Username zaten kullanılıyor mu kontrol et
        while (await userManager.FindByNameAsync(username) != null)
        {
            username = $"{baseUsername}{suffix}";
            suffix++;
        }

        return username;
    }
}
