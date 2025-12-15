namespace WordMaster.Application.Responses.Auth;

/// <summary>
/// Google OAuth callback sonrası dönen token bilgileri.
/// </summary>
public class LoginTokenResult
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
