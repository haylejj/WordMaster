using WordMaster.Application.Requests.Auth;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface ILoginService
{
    Task<ServiceResult<UserViewModel>> FindByEmailAsync(string email);
    Task<ServiceResult> LoginAsync(LoginRequest request);
    Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string userId);
}
