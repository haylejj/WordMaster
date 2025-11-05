using Core.Entity;
using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Identity;

namespace Service.Service;

public class LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ILoginService
{
    public async Task<AppUser> FindByEmailAsync(string email)
    {
        return await userManager.FindByEmailAsync(email);
    }
    public async Task<bool> LoginAsync(LoginRequest request, AppUser user)
    {
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);

        return result.Succeeded;
    }
    public async Task<string> GeneratePasswordResetTokenAsync(string userıd)
    {
        var user = await userManager.FindByIdAsync(userıd);
        return await userManager.GeneratePasswordResetTokenAsync(user);

    }


}
