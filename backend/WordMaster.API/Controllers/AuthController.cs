using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using WordMaster.API.Extensions;
using WordMaster.Application.Attributes;
using WordMaster.Application.Helpers;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Responses.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Kimlik doğrulama ve Jwt token işlemlerini yöneten controller.
/// Login, Register, Token yenileme gibi işlemleri içerir.
/// 
/// GÜVENLİK MODELİ:
/// - Access Token: Kısa ömürlü (15 dakika), response body'de döner, frontend memory'de saklar
/// - Refresh Token: Uzun ömürlü (7 gün), HttpOnly Secure cookie ile gönderilir, XSS'e karşı korumalı
/// </summary>
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("StrictPolicy")]
public class AuthController(
    ILoginService loginService,
    IRegisterService registerService,
    IUserService userService,
    IJwtService jwtService,
    IRefreshTokenCookieHelper cookieHelper,
    IGoogleAuthService googleAuthService) : BaseController
{

    /// <summary>
    /// API'nin ayakta olup olmadığını kontrol etmek için basit bir endpoint.
    /// </summary>
    /// <returns>200 OK durumu döner.</returns>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return CreateResult(ServiceResult.Success(HttpStatusCode.OK));
    }

    /// <summary>
    /// Access Token süresi dolduğunda, Refresh Token cookie'si kullanarak yeni bir Access Token alır.
    /// Refresh token HttpOnly cookie'den otomatik olarak okunur.
    /// </summary>
    /// <param name="request">Süresi dolmuş Access Token içeren istek.</param>
    /// <returns>Yeni Access Token bilgilerini döner. Yeni Refresh Token HttpOnly cookie olarak set edilir.</returns>
    /// <remarks>
    /// Bu endpoint, süresi dolmuş bir Access Token'ı yenilemek için kullanılır.
    /// İstemci, 401 Unauthorized hatası aldığında bu endpoint'e başvurmalıdır.
    /// Frontend sadece accessToken gönderir, refreshToken browser tarafından cookie olarak otomatik eklenir.
    /// </remarks>
    /// <response code="200">Token yenileme başarılı.</response>
    /// <response code="401">Refresh Token geçersiz veya süresi dolmuş.</response>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        ServiceResult<RefreshTokenResponse> result = await jwtService.RefreshAccessTokenWithCookieAsync(request.AccessToken);
        return CreateResult(result);
    }

    /// <summary>
    /// Geçerli access token'ı doğrulamak için basit bir endpoint.
    /// </summary>
    /// <remarks>
    /// Frontend bu endpoint'i belirli aralıklarla çağırarak kullanıcının oturumunun hâlâ geçerli olup olmadığını kontrol eder.
    /// Token geçerliyse 200 döner, aksi halde 401 döner.
    /// </remarks>
    [Authorize]
    [HttpGet("session-check")]
    public IActionResult SessionCheck()
    {
        return CreateResult(ServiceResult.Success(HttpStatusCode.OK));
    }

    /// <summary>
    /// Kullanıcı giriş işlemini gerçekleştirir ve JWT token döndürür.
    /// </summary>
    /// <param name="request">Email, şifre ve beni hatırla bilgilerini içeren login request</param>
    /// <returns>
    /// Başarılı durumda AccessToken içeren LoginResponse döner.
    /// RefreshToken HttpOnly cookie olarak set edilir (response body'de yer almaz).
    /// Başarısız durumda hata mesajı ve uygun HTTP status code döner.
    /// </returns>
    /// <remarks>
    /// Bu endpoint kullanıcı kimlik doğrulaması yapar ve başarılı olursa:
    /// - JWT Access Token (kısa süreli, API isteklerinde kullanılır) - Response body'de
    /// - Refresh Token (uzun süreli, access token yenilemek için kullanılır) - HttpOnly Cookie'de
    /// döndürür.
    /// 
    /// Ayrıca her giriş denemesi LogHistory tablosuna kaydedilir.
    /// </remarks>
    /// <response code="200">Giriş başarılı, token bilgileri döndürülür</response>
    /// <response code="401">Email veya şifre yanlış</response>
    /// <response code="403">Hesap kilitli veya IP adresi engellenmiş</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        ServiceResult<LoginResponse> result = await loginService.LoginWithCookieAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Admin girişi yapar ve JWT token döndürür.
    /// </summary>
    /// <param name="request">Email, şifre ve beni hatırla bilgilerini içeren login request</param>
    /// <returns>
    /// Başarılı durumda AccessToken içeren LoginResponse döner.
    /// RefreshToken HttpOnly cookie olarak set edilir.
    /// Başarısız durumda hata mesajı ve uygun HTTP status code döner.
    /// </returns>
    /// <remarks>
    /// Bu endpoint admin kimlik doğrulaması yapar. Normal kullanıcılar bu endpoint üzerinden giriş yapamaz.
    /// Ayrıca IP adresi kontrolü yapılır.
    /// </remarks>
    /// <response code="200">Giriş başarılı, token bilgileri döndürülür</response>
    /// <response code="401">Email veya şifre yanlış</response>
    /// <response code="403">Yetkisiz erişim (Admin değil veya IP engelli)</response>
    [HttpPost("admin-login")]
    public async Task<IActionResult> AdminLogin([FromBody] LoginRequest request)
    {
        ServiceResult<LoginResponse> result = await loginService.AdminLoginWithCookieAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Şifremi unuttum işlemi için şifre sıfırlama linki gönderir.
    /// </summary>
    /// <param name="request">Kullanıcının email adresini içeren istek.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    /// <remarks>
    /// Bu endpoint, verilen email adresi sistemde kayıtlıysa o adrese şifre sıfırlama bağlantısı içeren bir e-posta gönderir.
    /// Güvenlik nedeniyle, e-posta adresi sistemde kayıtlı olmasa bile başarılı sonuç döner (User Enumeration saldırılarını önlemek için).
    /// </remarks>
    /// <response code="200">İşlem başarılı (Mail gönderildi veya kullanıcı bulunamadı)</response>
    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        ServiceResult result = await loginService.ForgetPasswordAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Şifre sıfırlama işlemini gerçekleştirir.
    /// </summary>
    /// <param name="request">Şifreli kullanıcı ID'si, token ve yeni şifre bilgilerini içeren istek.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    /// <response code="200">Şifre başarıyla sıfırlandı.</response>
    /// <response code="400">Geçersiz istek (token hatalı, şifreler uyuşmuyor veya kullanıcı bulunamadı).</response>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        ServiceResult result = await loginService.ResetPasswordAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Yeni bir kullanıcı kaydı oluşturur.
    /// </summary>
    /// <param name="request">Kullanıcı adı, email, şifre telefon bilgileri ve cinsiyet içeren kayıt isteği.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    /// <remarks>
    /// Bu endpoint yeni bir kullanıcı oluşturur ve varsayılan olarak 'user' rolünü atar.
    /// </remarks>
    /// <response code="201">Kullanıcı başarıyla oluşturuldu.</response>
    /// <response code="400">Geçersiz istek veya validasyon hatası.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        ServiceResult result = await registerService.RegisterAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Email doğrulama işlemini gerçekleştirir.
    /// </summary>
    /// <param name="userId">Şifrelenmiş kullanıcı ID'si.</param>
    /// <param name="token">Email doğrulama token'ı.</param>
    /// <returns>Doğrulama sonucunu döner.</returns>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        ServiceResult result = await registerService.ConfirmEmailAsync(userId, token);
        return CreateResult(result);
    }

    /// <summary>
    /// Giriş yapmış kullanıcının şifresini değiştirir.
    /// </summary>
    /// <param name="request">Eski ve yeni şifre bilgilerini içeren istek.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    /// <remarks>
    /// Şifre değiştirme işlemi başarılı olursa, kullanıcının Security Stamp değeri güncellenir.
    /// Bu işlem, mevcut tüm JWT token'larını (Access Token) geçersiz kılar.
    /// Kullanıcının yeni şifresiyle tekrar giriş yapması gerekir.
    /// </remarks>
    /// <response code="204">Şifre başarıyla değiştirildi.</response>
    /// <response code="400">Eski şifre yanlış veya yeni şifre kurallara uymuyor.</response>
    /// <response code="401">Yetkisiz erişim.</response>
    /// <response code="404">Kullanıcı bulunamadı.</response>
    [HttpPost("change-password")]
    [Authorize]
    [RequirePermission("Public", "Auth", "ChangePassword", "POST", "Şifre değiştir")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        string userId = User.GetUserId().ToString();
        ServiceResult result = await userService.ChangePasswordAsync(request, userId);

        if (result.IsSuccess)
        {
            // Şifre değişti, refresh token cookie'sini sil
            cookieHelper.DeleteRefreshTokenCookie(HttpContext);
        }

        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcı çıkış işlemini gerçekleştirir.
    /// </summary>
    /// <returns>İşlem sonucunu döner.</returns>
    /// <remarks>
    /// Bu endpoint, kullanıcının sunucu tarafındaki Refresh Token'ını siler.
    /// Ayrıca client tarafındaki Refresh Token cookie'si de silinir.
    /// Access Token süresi dolduğunda kullanıcı yeni bir token alamaz ve tekrar giriş yapması gerekir.
    /// </remarks>
    /// <response code="200">Çıkış işlemi başarılı.</response>
    /// <response code="401">Yetkisiz erişim (Token geçersiz veya yok).</response>
    [HttpPost("logout")]
    [Authorize]
    [RequirePermission("Public", "Auth", "Logout", "POST", "Çıkış yap")]
    public async Task<IActionResult> Logout()
    {
        string? userName = User.GetUserName();
        if (string.IsNullOrEmpty(userName))
        {
            return CreateResult(ServiceResult.Failure("Kullanıcı adı bulunamadı.", HttpStatusCode.Unauthorized));
        }

        // Sunucu tarafında refresh token'ı sil
        ServiceResult result = await loginService.LogoutAsync(userName);

        // Client tarafındaki cookie'yi sil
        cookieHelper.DeleteRefreshTokenCookie(HttpContext);

        return CreateResult(result);
    }

    /// <summary>
    /// Google OAuth login sayfasına yönlendirir.
    /// </summary>
    /// <returns>Google consent sayfasına redirect.</returns>
    /// <remarks>
    /// Bu endpoint kullanıcıyı Google hesabıyla giriş yapması için
    /// Google OAuth consent sayfasına yönlendirir.
    /// </remarks>
    /// <response code="302">Google OAuth sayfasına yönlendirme.</response>
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        string authUrl = googleAuthService.GetGoogleAuthUrl();
        return Redirect(authUrl);
    }

    /// <summary>
    /// Google OAuth callback endpoint'i.
    /// </summary>
    /// <param name="code">Google'dan dönen authorization code.</param>
    /// <returns>Frontend'e token ile redirect.</returns>
    /// <remarks>
    /// Bu endpoint Google'dan dönen authorization code'u işler,
    /// kullanıcıyı bulur veya oluşturur ve JWT token üretir.
    /// Son olarak kullanıcıyı frontend'e token ile yönlendirir.
    /// </remarks>
    /// <response code="302">Frontend'e token ile yönlendirme.</response>
    /// <response code="400">Geçersiz authorization code.</response>
    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return Redirect($"http://localhost:5173/login?error=google_auth_failed");
        }

        ServiceResult<LoginTokenResult> result = await googleAuthService.HandleGoogleCallbackAsync(code);

        if (!result.IsSuccess || result.Data == null)
        {
            string errorMessage = result.ErrorList?.FirstOrDefault() ?? "google_auth_failed";
            return Redirect($"http://localhost:5173/login?error={Uri.EscapeDataString(errorMessage)}");
        }

        // Frontend'e token ile redirect
        // Token'ı URL'de geçmek yerine, cookie zaten set edildiği için sadece success flag gönderelim
        string redirectUrl = $"http://localhost:5173/auth/google-callback?token={Uri.EscapeDataString(result.Data.AccessToken)}&expiresAt={Uri.EscapeDataString(result.Data.ExpiresAt.ToString("o"))}";
        return Redirect(redirectUrl);
    }
}
