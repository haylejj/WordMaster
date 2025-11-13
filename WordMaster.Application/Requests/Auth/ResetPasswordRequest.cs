namespace WordMaster.Application.Requests.Auth;

public class ResetPasswordRequest
{
    public string? Password { get; set; }
    public string? PasswordConfirm { get; set; }
}

