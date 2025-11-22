using Microsoft.AspNetCore.Mvc;
using System.Net;
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
public class AuthController(ILoginService loginService) : BaseController
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
}
