using WordMaster.Domain.Entities;

namespace WordMaster.Application.Requests.Auth;

public class RegisterRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PasswordConfirm { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public Gender Gender { get; set; }
}

