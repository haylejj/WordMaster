using Core.Entity;
using Core.Requests;
using Core.Results;

namespace Core.Service;

public interface ILoginService
{
    Task<Result<AppUser>> FindByEmailAsync(string email);
    Task<Result> LoginAsync(LoginRequest request, AppUser user);
    Task<Result<string>> GeneratePasswordResetTokenAsync(string userıd);
}
