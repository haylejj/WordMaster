namespace WordMaster.Application.Requests.Auth;

public class ChangePasswordRequest
{
    public string PasswordOld { get; set; } = null!;
    public string PasswordNew { get; set; } = null!;
    public string PasswordConfirm { get; set; } = null!;
}

