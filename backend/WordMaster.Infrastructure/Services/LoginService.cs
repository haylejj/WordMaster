using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using WordMaster.Application.Helpers;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Responses.Auth;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Configuration;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Helpers;
using WordMaster.Domain.Results;

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
    IAllowedIpAddressService allowedIpAddressService,
    IRefreshTokenCookieHelper cookieHelper,
    IOptions<UrlsSettings> urlSettings,
    IOptions<JwtSettings> jwtSettings,
    ILogger<LoginService> logger) : ILoginService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

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
    public async Task<ServiceResult<LoginInternalResponse>> LoginAsync(LoginRequest request)
    {
        return await ProcessLoginAsync(request, isAdminLogin: false);
    }

    /// <summary>
    /// Admin girişi yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>Başarılı giriş durumunda token bilgilerini döner.</returns>
    public async Task<ServiceResult<LoginInternalResponse>> AdminLoginAsync(LoginRequest request)
    {
        return await ProcessLoginAsync(request, isAdminLogin: true);
    }

    /// <summary>
    /// Ortak login işlemlerini gerçekleştirir.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <param name="isAdminLogin">Admin girişi mi?</param>
    /// <returns>Başarılı giriş durumunda token bilgilerini döner.</returns>
    private async Task<ServiceResult<LoginInternalResponse>> ProcessLoginAsync(LoginRequest request, bool isAdminLogin)
    {
        string loginType = isAdminLogin ? "AdminLogin" : "PublicLogin";
        string? ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        // Admin girişi için IP kontrolü
        if (isAdminLogin && !string.IsNullOrWhiteSpace(ipAddress))
        {
            ServiceResult<bool> ipCheckResult = await allowedIpAddressService.IsIpAllowedAsync(ipAddress);
            if (!ipCheckResult.IsSuccess || !ipCheckResult.Data)
            {
                await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, loginType);
                return ServiceResult<LoginInternalResponse>.Failure("Bu IP adresinden admin paneline giriş yapma yetkiniz yok.", HttpStatusCode.Forbidden);
            }
        }

        // Kullanıcıyı bul
        AppUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, loginType);
            return ServiceResult<LoginInternalResponse>.Failure(
                isAdminLogin ? "Kullanıcı bulunamadı." : "Email veya şifre yanlış",
                isAdminLogin ? HttpStatusCode.NotFound : HttpStatusCode.NotFound);
        }

        // Admin girişi için rol kontrolü
        if (isAdminLogin)
        {
            bool isAdmin = await userManager.IsInRoleAsync(user, "admin");
            if (!isAdmin)
            {
                await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, loginType);
                return ServiceResult<LoginInternalResponse>.Failure("Bu panele erişim yetkiniz yok.", HttpStatusCode.Forbidden);
            }
        }

        // Şifre kontrolü
        SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password!, true);

        if (result.IsLockedOut)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, loginType);
            return ServiceResult<LoginInternalResponse>.Failure("Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyiniz.", HttpStatusCode.Forbidden);
        }

        if (!result.Succeeded)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, loginType);
            return ServiceResult<LoginInternalResponse>.Failure("Email veya şifre yanlış", HttpStatusCode.Unauthorized);
        }

        // Token'ları oluştur ve döndür
        return await GenerateTokensAndCompleteLoginAsync(user, request.Email, ipAddress, loginType);
    }

    /// <summary>
    /// Token'ları oluşturur ve login işlemini tamamlar.
    /// </summary>
    private async Task<ServiceResult<LoginInternalResponse>> GenerateTokensAndCompleteLoginAsync(
        AppUser user, string email, string? ipAddress, string loginType)
    {
        // Refresh token oluştur
        ServiceResult<string> refreshTokenResult = jwtService.GenerateRefreshToken();

        // Refresh token'ı HASH'leyerek veritabanına kaydet
        user.RefreshToken = RefreshTokenHasher.HashRefreshToken(refreshTokenResult.Data!);
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays);
        await userManager.UpdateAsync(user);

        // SecurityStamp cache'ini set et
        await cacheService.SetAsync($"security_stamp:{user.Id}", user.SecurityStamp, TimeSpan.FromHours(1));

        IList<string> roles = await userManager.GetRolesAsync(user);

        // Access token oluştur
        ServiceResult<string> accessTokenResult = jwtService.GenerateAccessToken(
            user.Id.ToString(), user.UserName!, user.Email!, roles, user.SecurityStamp!);

        if (!accessTokenResult.IsSuccess)
        {
            await logHistoryService.RecordAsync(user.Id.ToString(), email, ipAddress, false, loginType);
            return ServiceResult<LoginInternalResponse>.Failure("Token oluşturulamadı.", HttpStatusCode.InternalServerError);
        }

        // Başarılı login kaydı
        await logHistoryService.RecordAsync(user.Id.ToString(), email, ipAddress, true, loginType);

        logger.LogInformation("{LoginType}: User {UserId} logged in successfully from IP {IpAddress}",
            loginType, user.Id, ipAddress);

        // Plain refresh token'ı döndür (Controller cookie olarak set edecek)
        return ServiceResult<LoginInternalResponse>.Success(new LoginInternalResponse
        {
            AccessToken = accessTokenResult.Data!,
            RefreshToken = refreshTokenResult.Data!  // Plain text, cookie için
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

        //string baseUrl = $"{requestContext?.Scheme}://{requestContext?.Host}";
        string baseUrl = urlSettings.Value.Client;

        // UserId'yi şifrele
        string encryptedUserId = dataProtectionHelper.Encrypt(user.Id.ToString());
        string encodedEncryptedUserId = WebUtility.UrlEncode(encryptedUserId);

        string passwordResetLink = $"{baseUrl}/ResetPassword?userId={encodedEncryptedUserId}&token={WebUtility.UrlEncode(token)}";

        ServiceResult emailResult = await emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink, user.Email!);

        if (emailResult.IsSuccess)
        {
            logger.LogInformation("Password reset link sent successfully to {Email}", user.Email);
            return ServiceResult.Success(HttpStatusCode.OK);
        }
        else
        {
            return ServiceResult.Failure("Şifre sıfırlama e-postası gönderilemedi.", HttpStatusCode.InternalServerError);
        }
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
            logger.LogWarning("Password reset failed for user {UserId}. Errors: {Errors}", user.Id, string.Join(", ", errors));
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        // Güvenlik damgasını güncelle (eski oturumları sonlandırabilir)
        await userManager.UpdateSecurityStampAsync(user);
        // SecurityStamp cache'ini temizle
        await cacheService.RemoveAsync($"security_stamp:{user.Id}");

        logger.LogInformation("Password reset successfully for user {UserId}", user.Id);

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

        logger.LogInformation("User {UserId} logged out successfully", user.Id);

        return ServiceResult.Success(HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcı girişi yapar ve refresh token'ı cookie'ye yazar.
    /// </summary>
    public async Task<ServiceResult<LoginResponse>> LoginWithCookieAsync(LoginRequest request)
    {
        ServiceResult<LoginInternalResponse> result = await LoginAsync(request);

        if (!result.IsSuccess || result.Data == null)
        {
            return ServiceResult<LoginResponse>.Failure(result.ErrorList ?? [], result.StatusCode);
        }

        // Cookie'ye refresh token yaz
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            cookieHelper.SetRefreshTokenCookie(httpContext, result.Data.RefreshToken);
        }

        return ServiceResult<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = result.Data.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes)
        }, result.StatusCode);
    }

    /// <summary>
    /// Admin girişi yapar ve refresh token'ı cookie'ye yazar.
    /// </summary>
    public async Task<ServiceResult<LoginResponse>> AdminLoginWithCookieAsync(LoginRequest request)
    {
        ServiceResult<LoginInternalResponse> result = await AdminLoginAsync(request);

        if (!result.IsSuccess || result.Data == null)
        {
            return ServiceResult<LoginResponse>.Failure(result.ErrorList ?? [], result.StatusCode);
        }

        // Cookie'ye refresh token yaz
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            cookieHelper.SetRefreshTokenCookie(httpContext, result.Data.RefreshToken);
        }

        return ServiceResult<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = result.Data.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes)
        }, result.StatusCode);
    }
}
