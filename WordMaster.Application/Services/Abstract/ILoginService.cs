using WordMaster.Application.Requests.Auth;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface ILoginService
{
    Task<ServiceResult<AppUser>> FindByEmailAsync(string email);
    Task<ServiceResult> LoginAsync(LoginRequest request, AppUser user);
    Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string userId);

}
