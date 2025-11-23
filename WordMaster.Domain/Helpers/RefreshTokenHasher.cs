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
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Gelen refresh token'ı hash'leyip veritabanındaki hash ile karşılaştırır.
    /// </summary>
    /// <param name="providedToken">Kullanıcıdan gelen plain text token</param>
    /// <param name="storedHash">Veritabanında saklanan hash</param>
    /// <returns>Eşleşiyorsa true</returns>
    public static bool VerifyRefreshToken(string providedToken, string storedHash)
    {
        string providedHash = HashRefreshToken(providedToken);
        return providedHash == storedHash;
    }
}
