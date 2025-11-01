using Core.Entity;
using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Identity;

namespace Service.Service;

public class LoginService : ILoginService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;


    public LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;

    }
    public async Task<AppUser> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }
    public async Task<bool> LoginAsync(LoginRequest request, AppUser user)
    {
        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);

        return result.Succeeded;
    }
    public async Task<string> GeneratePasswordResetTokenAsync(string userıd)
    {
        var user = await _userManager.FindByIdAsync(userıd);
        return await _userManager.GeneratePasswordResetTokenAsync(user);

    }


}
