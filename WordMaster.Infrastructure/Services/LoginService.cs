using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ILoginService
{
    public async Task<Result<AppUser>> FindByEmailAsync(string email)
    {
        AppUser? user = await userManager.FindByEmailAsync(email);
        return user == null ? Result<AppUser>.Failure("Kullanıcı bulunamadı.") : Result<AppUser>.Success(user);
    }
    public async Task<Result> LoginAsync(LoginRequest request, AppUser user)
    {
        SignInResult result = await signInManager.PasswordSignInAsync(user, request.Password!, request.RememberMe, true);
        return result.Succeeded ? Result.Success() : Result.Failure("Email veya şifre yanlış");
    }
    public async Task<Result<string>> GeneratePasswordResetTokenAsync(string userId)
    {
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null) return Result<string>.Failure("Kullanıcı bulunamadı.");
        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        return Result<string>.Success(token);

    }


}
