namespace WordMaster.Application.Requests.Auth;

public class ResetPasswordRequest
{
    public string Password { get; set; } = null!;
    public string PasswordConfirm { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = null!;
}

