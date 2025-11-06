using Core.Entity;
using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Identity;
using Core.Results;

namespace Service.Service;

public class LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ILoginService
{
    public async Task<Result<AppUser>> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user == null ? Result<AppUser>.Failure("Kullanıcı bulunamadı.") : Result<AppUser>.Success(user);
    }
    public async Task<Result> LoginAsync(LoginRequest request, AppUser user)
    {
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
        return result.Succeeded ? Result.Success() : Result.Failure("Email veya şifre yanlış");
    }
    public async Task<Result<string>> GeneratePasswordResetTokenAsync(string userıd)
    {
        var user = await userManager.FindByIdAsync(userıd);
        if (user == null) return Result<string>.Failure("Kullanıcı bulunamadı.");
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return Result<string>.Success(token);

    }


}
