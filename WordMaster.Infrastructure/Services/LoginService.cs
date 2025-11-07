using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ILoginService
{
    public async Task<Result<AppUser>> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user == null ? Result<AppUser>.Failure("Kullanýcý bulunamadý.") : Result<AppUser>.Success(user);
    }
    public async Task<Result> LoginAsync(LoginRequest request, AppUser user)
    {
        var result = await signInManager.PasswordSignInAsync(user, request.Password!, request.RememberMe, true);
        return result.Succeeded ? Result.Success() : Result.Failure("Email veya þifre yanlýþ");
    }
    public async Task<Result<string>> GeneratePasswordResetTokenAsync(string userýd)
    {
        var user = await userManager.FindByIdAsync(userýd);
        if (user == null) return Result<string>.Failure("Kullanýcý bulunamadý.");
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return Result<string>.Success(token);

    }


}
