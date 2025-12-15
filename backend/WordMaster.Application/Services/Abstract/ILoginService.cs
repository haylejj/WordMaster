using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Responses.Auth;
using WordMaster.Application.Responses.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface ILoginService
{
    /// <summary>
    /// Verilen email adresiyle kullanıcıyı bulur.
    /// </summary>
    /// <param name="email">Aranacak kullanıcının email adresi.</param>
    /// <returns>Kullanıcı bulunursa kullanıcı bilgilerini, bulunamazsa hata döner.</returns>
    Task<ServiceResult<UserResponse>> FindByEmailAsync(string email);

    /// <summary>
    /// Kullanıcı girişi yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>
    /// Başarılı giriş durumunda LoginInternalResponse döner.
    /// Controller bu bilgiyi kullanarak access token'ı body'de, refresh token'ı cookie'de gönderir.
    /// </returns>
    Task<ServiceResult<LoginInternalResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Kullanıcı girişi yapar ve refresh token'ı cookie'ye yazar.
    /// Cookie işlemlerini servis içinde yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>Başarılı giriş durumunda LoginResponse döner.</returns>
    Task<ServiceResult<LoginResponse>> LoginWithCookieAsync(LoginRequest request);

    /// <summary>
    /// Admin girişi yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>
    /// Başarılı giriş durumunda LoginInternalResponse döner.
    /// Controller bu bilgiyi kullanarak access token'ı body'de, refresh token'ı cookie'de gönderir.
    /// </returns>
    Task<ServiceResult<LoginInternalResponse>> AdminLoginAsync(LoginRequest request);

    /// <summary>
    /// Admin girişi yapar ve refresh token'ı cookie'ye yazar.
    /// Cookie işlemlerini servis içinde yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>Başarılı giriş durumunda LoginResponse döner.</returns>
    Task<ServiceResult<LoginResponse>> AdminLoginWithCookieAsync(LoginRequest request);

    /// <summary>
    /// Şifre sıfırlama tokeni oluşturur.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <returns>Oluşturulan tokeni döner.</returns>
    Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string userId);

    /// <summary>
    /// Şifremi unuttum işlemi için kullanıcıya şifre sıfırlama linki gönderir.
    /// </summary>
    /// <param name="request">Email bilgisini içeren istek modeli.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    Task<ServiceResult> ForgetPasswordAsync(ForgetPasswordRequest request);

    /// <summary>
    /// Şifre sıfırlama işlemini gerçekleştirir.
    /// </summary>
    /// <param name="request">Şifre sıfırlama bilgilerini içeren istek.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Kullanıcı çıkış işlemini gerçekleştirir (Refresh token'ı siler).
    /// </summary>
    /// <param name="userName">Çıkış yapacak kullanıcının kullanıcı adı.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    Task<ServiceResult> LogoutAsync(string userName);
}
