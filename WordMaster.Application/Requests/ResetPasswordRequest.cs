namespace WordMaster.Application.Requests;

public class ResetPasswordRequest
{
    public string? Password { get; set; }
    public string? PasswordConfirm { get; set; }
}

