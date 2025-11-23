using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WordMaster.API.Extensions;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Responses;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Kimlik doğrulama ve Jwt token işlemlerini yöneten controller.
/// Login, Register, Token yenileme gibi işlemleri içerir.
/// </summary>
[Route("api/auth")]
public class AuthController(ILoginService loginService, IRegisterService registerService, IUserService userService, IJwtService jwtService) : BaseController
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
    /// Access Token süresi dolduğunda, Refresh Token kullanarak yeni bir Access Token alır.
    /// </summary>
    /// <param name="request">Süresi dolmuş Access Token ve geçerli Refresh Token içeren istek.</param>
    /// <returns>Yeni Access Token ve Refresh Token bilgilerini döner.</returns>
    /// <remarks>
    /// Bu endpoint, süresi dolmuş bir Access Token'ı yenilemek için kullanılır.
    /// İstemci, 401 Unauthorized hatası aldığında (veya token süresinin dolduğunu fark ettiğinde) bu endpoint'e başvurmalıdır.
    /// </remarks>
    /// <response code="200">Token yenileme başarılı.</response>
    /// <response code="401">Refresh Token geçersiz veya süresi dolmuş.</response>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        ServiceResult<RefreshTokenResponse> result = await jwtService.RefreshAccessTokenAsync(request.AccessToken, request.RefreshToken);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcı giriş işlemini gerçekleştirir ve JWT token döndürür.
    /// </summary>
    /// <param name="request">Email, şifre ve beni hatırla bilgilerini içeren login request</param>
    /// <returns>
    /// Başarılı durumda AccessToken ve RefreshToken içeren LoginResponse döner.
    /// Başarısız durumda hata mesajı ve uygun HTTP status code döner.
    /// </returns>
    /// <remarks>
    /// Bu endpoint kullanıcı kimlik doğrulaması yapar ve başarılı olursa:
    /// - JWT Access Token (kısa süreli, API isteklerinde kullanılır)
    /// - Refresh Token (uzun süreli, access token yenilemek için kullanılır)
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
        ServiceResult<LoginResponse> result = await loginService.LoginAsync(request);
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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        string? userName = User.GetUserName();
        if (string.IsNullOrEmpty(userName))
        {
            return CreateResult(ServiceResult.Failure("Kullanıcı adı bulunamadı.", HttpStatusCode.Unauthorized));
        }

        ServiceResult result = await userService.ChangePasswordAsync(request, userName);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcı çıkış işlemini gerçekleştirir.
    /// </summary>
    /// <returns>İşlem sonucunu döner.</returns>
    /// <remarks>
    /// Bu endpoint, kullanıcının sunucu tarafındaki Refresh Token'ını siler.
    /// Böylece Access Token süresi dolduğunda kullanıcı yeni bir token alamaz ve tekrar giriş yapması gerekir.
    /// Client tarafında da Access Token ve Refresh Token silinmelidir.
    /// </remarks>
    /// <response code="200">Çıkış işlemi başarılı.</response>
    /// <response code="401">Yetkisiz erişim (Token geçersiz veya yok).</response>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        string? userName = User.GetUserName();
        if (string.IsNullOrEmpty(userName))
        {
            return CreateResult(ServiceResult.Failure("Kullanıcı adı bulunamadı.", HttpStatusCode.Unauthorized));
        }

        ServiceResult result = await loginService.LogoutAsync(userName);
        return CreateResult(result);
    }
}
