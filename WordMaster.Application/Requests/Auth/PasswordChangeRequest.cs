namespace WordMaster.Application.Requests.Auth;

public class PasswordChangeRequest
{
    public string? PasswordOld { get; set; }
    public string? PasswordNew { get; set; }
    public string? PasswordConfirm { get; set; }
}

