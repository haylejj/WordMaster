using WordMaster.Domain.Results;
using WordMaster.Application.Responses.Auth;

namespace WordMaster.Application.Services.Abstract;

/// <summary>
/// Google OAuth 2.0 ile kimlik doğrulama işlemlerini yöneten servis interface.
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Google OAuth yetkilendirme URL'ini oluşturur.
    /// </summary>
    /// <returns>Google consent sayfasına yönlendirme URL'i.</returns>
    string GetGoogleAuthUrl();

    /// <summary>
    /// Google'dan dönen authorization code ile kullanıcı girişi/kaydı yapar.
    /// </summary>
    /// <param name="code">Google'dan alınan authorization code.</param>
    /// <returns>JWT token içeren login response.</returns>
    Task<ServiceResult<LoginTokenResult>> HandleGoogleCallbackAsync(string code);
}


