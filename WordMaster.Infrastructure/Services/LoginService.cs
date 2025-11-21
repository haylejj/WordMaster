using System.Net;
using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ILoginService
{
    public async Task<ServiceResult<UserViewModel>> FindByEmailAsync(string email)
    {
        AppUser? user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ServiceResult<UserViewModel>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        UserViewModel userViewModel = new()
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!
        };

        return ServiceResult<UserViewModel>.Success(userViewModel, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ServiceResult.Failure("Email adresi gereklidir.", HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        SignInResult result = await signInManager.PasswordSignInAsync(user, request.Password!, request.RememberMe, true);

        if (result.IsLockedOut)
        {
            return ServiceResult.Failure("Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyiniz.", HttpStatusCode.Forbidden);
        }

        return result.Succeeded
            ? ServiceResult.Success(HttpStatusCode.OK)
            : ServiceResult.Failure("Email veya şifre yanlış", HttpStatusCode.Unauthorized);
    }

    public async Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string userId)
    {
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<string>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        string token = await userManager.GeneratePasswordResetTokenAsync(user);
        return ServiceResult<string>.Success(token, HttpStatusCode.OK);
    }
}
