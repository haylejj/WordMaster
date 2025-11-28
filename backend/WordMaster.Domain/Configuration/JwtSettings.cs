namespace WordMaster.Domain.Configuration;

/// <summary>
/// JWT token yapılandırma ayarları.
/// </summary>
public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
    public int RefreshTokenExpiresInDays { get; set; }
}
