using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Responses;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface ILoginService
{
    /// <summary>
    /// Verilen email adresiyle kullanıcıyı bulur.
    /// </summary>
    /// <param name="email">Aranacak kullanıcının email adresi.</param>
    /// <returns>Kullanıcı bulunursa kullanıcı bilgilerini, bulunamazsa hata döner.</returns>
    Task<ServiceResult<UserViewModel>> FindByEmailAsync(string email);

    /// <summary>
    /// Kullanıcı girişi yapar.
    /// </summary>
    /// <param name="request">Giriş bilgilerini içeren model.</param>
    /// <returns>Başarılı giriş durumunda token bilgilerini döner.</returns>
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);

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
}
