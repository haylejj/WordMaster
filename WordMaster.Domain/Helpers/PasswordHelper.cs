using System.Security.Cryptography;

namespace WordMaster.Domain.Helpers;

public static class PasswordHelper
{
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const string Numbers = "0123456789";

    /// <summary>
    /// Kriptografik olarak güvenli rastgele şifre oluşturur.
    /// Şifre en az 1 harf ve 1 sayı içerir ve toplam 8 karakter uzunluğundadır.
    /// </summary>
    /// <returns>8 karakterlik rastgele şifre (en az 1 harf ve 1 sayı içerir)</returns>
    public static string GenerateRandomPassword()
    {
        // Kriptografik olarak güvenli rastgele sayı üretici kullan
        using var rng = RandomNumberGenerator.Create();

        var passwordChars = new char[8];
        var allChars = Letters + Numbers;

        // En az 1 harf garantisi için 1 harf ekle
        passwordChars[0] = GetRandomChar(rng, Letters);

        // En az 1 sayı garantisi için 1 sayı ekle
        passwordChars[1] = GetRandomChar(rng, Numbers);

        // Kalan 6 karakteri rastgele harf veya sayı ile doldur
        for (int i = 2; i < 8; i++)
        {
            passwordChars[i] = GetRandomChar(rng, allChars);
        }

        // Fisher-Yates shuffle algoritması ile karakterleri karıştır
        ShuffleArray(rng, passwordChars);

        return new string(passwordChars);
    }

    /// <summary>
    /// Kriptografik olarak güvenli rastgele karakter seçer
    /// </summary>
    private static char GetRandomChar(RandomNumberGenerator rng, string chars)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomValue = BitConverter.ToUInt32(randomBytes, 0);
        return chars[(int)(randomValue % (uint)chars.Length)];
    }

    /// <summary>
    /// Fisher-Yates shuffle algoritması ile diziyi karıştırır
    /// </summary>
    private static void ShuffleArray(RandomNumberGenerator rng, char[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            var randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            var randomValue = BitConverter.ToUInt32(randomBytes, 0);
            int j = (int)(randomValue % (uint)(i + 1));

            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}

