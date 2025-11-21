using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ILoginService
{
    public async Task<ServiceResult<AppUser>> FindByEmailAsync(string email)
    {
        AppUser? user = await userManager.FindByEmailAsync(email);
        return user == null ? ServiceResult<AppUser>.Failure("Kullanıcı bulunamadı.") : ServiceResult<AppUser>.Success(user);
    }
    public async Task<ServiceResult> LoginAsync(LoginRequest request, AppUser user)
    {
        SignInResult result = await signInManager.PasswordSignInAsync(user, request.Password!, request.RememberMe, true);
        return result.Succeeded ? ServiceResult.Success() : ServiceResult.Failure("Email veya şifre yanlış");
    }
    public async Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string userId)
    {
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<string>.Failure("Kullanıcı bulunamadı.");
        }

        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        return ServiceResult<string>.Success(token);

    }


}
