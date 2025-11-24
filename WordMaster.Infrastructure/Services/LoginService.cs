using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Responses.Auth;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

using WordMaster.Infrastructure.Helpers;

namespace WordMaster.Infrastructure.Services;

public class LoginService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IJwtService jwtService,
    ILogHistoryService logHistoryService,
    IHttpContextAccessor httpContextAccessor,
    IEmailService emailService,
    IDataProtectionHelper dataProtectionHelper,
    ICacheService cacheService,
    IAllowedIpAddressService allowedIpAddressService) : ILoginService
{
    /// <summary>
    /// Verilen email adresiyle kullanıcıyı bulur.
    /// </summary>
    /// <param name="email">Aranacak kullanıcının email adresi.</param>
    /// <returns>Kullanıcı bulunursa kullanıcı bilgilerini, bulunamazsa hata döner.</returns>
    public async Task<ServiceResult<UserResponse>> FindByEmailAsync(string email)
    {
        AppUser? user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ServiceResult<UserResponse>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        UserResponse userViewModel = new()
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!
        };

        return ServiceResult<UserResponse>.Success(userViewModel, HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcı girişi yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>Başarılı giriş durumunda token bilgilerini döner.</returns>
    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        string? ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        AppUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "APILogin");
            return ServiceResult<LoginResponse>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password!, true);

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

        ServiceResult<string> refreshTokenResult = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshTokenResult.Data;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        // SecurityStamp cache'ini set et
        await cacheService.SetAsync($"security_stamp:{user.Id}", user.SecurityStamp, TimeSpan.FromHours(1));

        IList<string> roles = await userManager.GetRolesAsync(user);

        // Token'ı en son kullanıcı durumuyla oluştur (UpdateAsync sonrası)
        ServiceResult<string> accessTokenResult = jwtService.GenerateAccessToken(user.Id.ToString(), user.UserName!, user.Email!, roles, user.SecurityStamp!);

        if (!accessTokenResult.IsSuccess)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "PublicLogin");
            return ServiceResult<LoginResponse>.Failure("Token oluşturulamadı.", HttpStatusCode.InternalServerError);
        }

        // Başarılı login kaydı
        await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, true, "PublicLogin");

        return ServiceResult<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessTokenResult.Data!,
            RefreshToken = refreshTokenResult.Data!
        }, HttpStatusCode.OK);
    }

    /// <summary>
    /// Admin girişi yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>Başarılı giriş durumunda token bilgilerini döner.</returns>
    public async Task<ServiceResult<LoginResponse>> AdminLoginAsync(LoginRequest request)
    {
        string? ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        // IP adresi kontrolü
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            ServiceResult<bool> ipCheckResult = await allowedIpAddressService.IsIpAllowedAsync(ipAddress);
            if (!ipCheckResult.IsSuccess || !ipCheckResult.Data)
            {
                await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "AdminLogin");
                return ServiceResult<LoginResponse>.Failure("Bu IP adresinden admin paneline giriş yapma yetkiniz yok.", HttpStatusCode.Forbidden);
            }
        }

        AppUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "AdminLogin");
            return ServiceResult<LoginResponse>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        // Admin rol kontrolü
        bool isAdmin = await userManager.IsInRoleAsync(user, "admin");
        if (!isAdmin)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "AdminLogin");
            return ServiceResult<LoginResponse>.Failure("Bu panele erişim yetkiniz yok.", HttpStatusCode.Forbidden);
        }

        SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password!, true);

        if (result.IsLockedOut)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "AdminLogin");
            return ServiceResult<LoginResponse>.Failure("Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyiniz.", HttpStatusCode.Forbidden);
        }

        if (!result.Succeeded)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "AdminLogin");
            return ServiceResult<LoginResponse>.Failure("Email veya şifre yanlış", HttpStatusCode.Unauthorized);
        }

        ServiceResult<string> refreshTokenResult = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshTokenResult.Data;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        // SecurityStamp cache'ini set et
        await cacheService.SetAsync($"security_stamp:{user.Id}", user.SecurityStamp, TimeSpan.FromHours(1));

        IList<string> roles = await userManager.GetRolesAsync(user);

        // Token'ı en son kullanıcı durumuyla oluştur (UpdateAsync sonrası)
        ServiceResult<string> accessTokenResult = jwtService.GenerateAccessToken(user.Id.ToString(), user.UserName!, user.Email!, roles, user.SecurityStamp!);

        if (!accessTokenResult.IsSuccess)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "AdminLogin");
            return ServiceResult<LoginResponse>.Failure("Token oluşturulamadı.", HttpStatusCode.InternalServerError);
        }

        // Başarılı login kaydı
        await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, true, "AdminLogin");

        return ServiceResult<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessTokenResult.Data!,
            RefreshToken = refreshTokenResult.Data!
        }, HttpStatusCode.OK);
    }

    /// <summary>
    /// Şifre sıfırlama tokeni oluşturur.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <returns>Oluşturulan tokeni döner.</returns>
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

    /// <summary>
    /// Şifremi unuttum işlemi için kullanıcıya şifre sıfırlama linki gönderir.
    /// </summary>
    /// <param name="request">Email bilgisini içeren istek modeli.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    public async Task<ServiceResult> ForgetPasswordAsync(ForgetPasswordRequest request)
    {
        AppUser? user = await userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            // Güvenlik gereği kullanıcı bulunamadı demek yerine başarılı dönüyoruz.
            return ServiceResult.Success(HttpStatusCode.OK);
        }

        string token = await userManager.GeneratePasswordResetTokenAsync(user);

        HttpRequest? requestContext = httpContextAccessor.HttpContext?.Request;
        string baseUrl = $"{requestContext?.Scheme}://{requestContext?.Host}";

        // UserId'yi şifrele
        string encryptedUserId = dataProtectionHelper.Encrypt(user.Id.ToString());
        string encodedEncryptedUserId = WebUtility.UrlEncode(encryptedUserId);

        string passwordResetLink = $"{baseUrl}/ResetPassword?userId={encodedEncryptedUserId}&token={WebUtility.UrlEncode(token)}";

        ServiceResult emailResult = await emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink, user.Email!);
        return !emailResult.IsSuccess
            ? ServiceResult.Failure("Şifre sıfırlama e-postası gönderilemedi.", HttpStatusCode.InternalServerError)
            : ServiceResult.Success(HttpStatusCode.OK);
    }

    /// <summary>
    /// Şifre sıfırlama işlemini gerçekleştirir.
    /// </summary>
    /// <param name="request">Şifre sıfırlama bilgilerini içeren istek.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // UserId'yi çöz
        string decryptedUserId = dataProtectionHelper.Decrypt(request.UserId);
        if (string.IsNullOrEmpty(decryptedUserId))
        {
            return ServiceResult.Failure("Geçersiz kullanıcı bilgisi.", HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.FindByIdAsync(decryptedUserId);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        if (request.Password != request.PasswordConfirm)
        {
            return ServiceResult.Failure("Şifreler uyuşmuyor.", HttpStatusCode.BadRequest);
        }

        IdentityResult result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);

        if (!result.Succeeded)
        {
            List<string> errors = result.Errors.Select(x => x.Description).ToList();
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        // Güvenlik damgasını güncelle (eski oturumları sonlandırabilir)
        await userManager.UpdateSecurityStampAsync(user);
        // SecurityStamp cache'ini temizle
        await cacheService.RemoveAsync($"security_stamp:{user.Id}");

        return ServiceResult.Success(HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcı çıkış işlemini gerçekleştirir.
    /// </summary>
    /// <param name="userName">Çıkış yapacak kullanıcının kullanıcı adı.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    public async Task<ServiceResult> LogoutAsync(string userName)
    {
        AppUser? user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        // Refresh token'ı silerek kullanıcının yeni access token almasını engelle
        user.RefreshToken = null;
        user.RefreshTokenExpires = null;

        // Security Stamp'i güncelle (Bu işlem mevcut Access Token'ı anında geçersiz kılar)
        // Not: UpdateSecurityStampAsync, user nesnesindeki diğer değişiklikleri de (RefreshToken=null) kaydeder.
        await userManager.UpdateSecurityStampAsync(user);

        // SecurityStamp cache'ini temizle (Middleware yeni stamp'i DB'den okusun)
        await cacheService.RemoveAsync($"security_stamp:{user.Id}");

        return ServiceResult.Success(HttpStatusCode.OK);
    }
}
