using WordMaster.Application.Requests.Auth;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Results;
using WordMaster.Application.Responses;

namespace WordMaster.Application.Services.Abstract;

public interface ILoginService
{
    Task<ServiceResult<UserViewModel>> FindByEmailAsync(string email);
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string userId);
}
