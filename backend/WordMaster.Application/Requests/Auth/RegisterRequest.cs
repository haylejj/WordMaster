namespace WordMaster.Application.Requests.Auth;

public class RegisterRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PasswordConfirm { get; set; } = null!;
    public string Phone { get; set; } = null!;
}

