namespace WordMaster.Domain.Helpers;

/// <summary>
/// IP adresi işlemleri için yardımcı sınıf.
/// </summary>
public static class IpAddressHelper
{
    private const string Ipv6MappedPrefix = "::ffff:";

    /// <summary>
    /// IPv6-mapped IPv4 adreslerini normalize eder.
    /// Örnek: ::ffff:192.168.1.1 -> 192.168.1.1
    /// </summary>
    /// <param name="ipAddress">Normalize edilecek IP adresi</param>
    /// <returns>Normalize edilmiş IP adresi</returns>
    public static string? Normalize(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return ipAddress;

        if (ipAddress.StartsWith(Ipv6MappedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ipAddress.Substring(Ipv6MappedPrefix.Length);
        }

        return ipAddress;
    }
}
