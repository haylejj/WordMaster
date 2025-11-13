using WordMaster.Application.Requests.Auth;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface ILoginService
{
    Task<Result<AppUser>> FindByEmailAsync(string email);
    Task<Result> LoginAsync(LoginRequest request, AppUser user);
    Task<Result<string>> GeneratePasswordResetTokenAsync(string userId);

}
