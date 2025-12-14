using System.Security.Cryptography;
using System.Text;

namespace WordMaster.Domain.Helpers;

/// <summary>
/// Refresh token güvenliği için hash işlemleri.
/// </summary>
public static class RefreshTokenHasher
{
    /// <summary>
    /// Refresh token'ı SHA256 ile hash'ler.
    /// Veritabanında plain text yerine hash saklanır.
    /// </summary>
    /// <param name="refreshToken">Plain text refresh token</param>
    /// <returns>SHA256 hash (Base64 encoded)</returns>
    public static string HashRefreshToken(string refreshToken)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Gelen refresh token'ı hash'leyip veritabanındaki hash ile karşılaştırır.
    /// Timing attack'a karşı sabit zamanlı karşılaştırma kullanır.
    /// </summary>
    /// <param name="providedToken">Kullanıcıdan gelen plain text token</param>
    /// <param name="storedHash">Veritabanında saklanan hash</param>
    /// <returns>Eşleşiyorsa true</returns>
    public static bool VerifyRefreshToken(string providedToken, string storedHash)
    {
        string providedHash = HashRefreshToken(providedToken);

        // Timing attack'a karşı sabit zamanlı karşılaştırma
        // Normal string karşılaştırması farklılık bulunduğu anda durur,
        // bu da saldırganın doğru karakterleri tahmin etmesine olanak tanır
        byte[] providedBytes = Convert.FromBase64String(providedHash);
        byte[] storedBytes = Convert.FromBase64String(storedHash);

        return CryptographicOperations.FixedTimeEquals(providedBytes, storedBytes);
    }
}
