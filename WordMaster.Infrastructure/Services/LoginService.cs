using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Responses;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class LoginService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IJwtService jwtService,
    ILogHistoryService logHistoryService,
    IHttpContextAccessor httpContextAccessor) : ILoginService
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

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        string? ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        AppUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "APILogin");
            return ServiceResult<LoginResponse>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        SignInResult result = await signInManager.PasswordSignInAsync(user, request.Password!, request.RememberMe, true);

        if (result.IsLockedOut)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "PublicLogin");
            return ServiceResult<LoginResponse>.Failure("Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyiniz.", HttpStatusCode.Forbidden);
        }

        if (!result.Succeeded)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "PublicLogin");
            return ServiceResult<LoginResponse>.Failure("Email veya şifre yanlış", HttpStatusCode.Unauthorized);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        ServiceResult<string> accessTokenResult = jwtService.GenerateAccessToken(user.Id.ToString(), user.Email!, roles, user.SecurityStamp!);

        if (!accessTokenResult.IsSuccess)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "PublicLogin");
            return ServiceResult<LoginResponse>.Failure("Token oluşturulamadı.", HttpStatusCode.InternalServerError);
        }

        ServiceResult<string> refreshTokenResult = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshTokenResult.Data;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        // Başarılı login kaydı
        await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, true, "PublicLogin");

        return ServiceResult<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessTokenResult.Data!,
            RefreshToken = refreshTokenResult.Data!
        }, HttpStatusCode.OK);
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
