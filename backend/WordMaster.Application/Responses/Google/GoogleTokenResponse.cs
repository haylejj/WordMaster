using System.Text.Json.Serialization;

namespace WordMaster.Application.Responses.Google;

/// <summary>
/// Google OAuth token response modeli.
/// </summary>
public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = null!;
}
