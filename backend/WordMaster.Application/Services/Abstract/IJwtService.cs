using System.Security.Claims;
using WordMaster.Application.Responses.Auth;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

/// <summary>
/// JWT token oluşturma ve yönetme işlemlerini sağlayan servis interface'i.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Kullanıcı için JWT access token oluşturur.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="userName">Kullanıcı adı</param>
    /// <param name="email">Kullanıcı email adresi</param>
    /// <param name="roles">Kullanıcının rolleri</param>
    /// <param name="securityStamp">Kullanıcının security stamp'i (şifre değişince token geçersiz olur)</param>
    /// <returns>JWT access token string içeren ServiceResult</returns>
    ServiceResult<string> GenerateAccessToken(string userId, string userName, string email, IList<string> roles, string securityStamp);

    /// <summary>
    /// Güvenli bir refresh token oluşturur.
    /// </summary>
    /// <returns>Rastgele oluşturulmuş refresh token string içeren ServiceResult</returns>
    ServiceResult<string> GenerateRefreshToken();

    /// <summary>
    /// Mevcut access token'dan ClaimsPrincipal çıkarır (token doğrulaması yapmadan).
    /// Refresh token işlemlerinde kullanılır.
    /// </summary>
    /// <param name="accessToken">Geçerlilik süresi dolmuş veya geçerli access token</param>
    /// <returns>Token içindeki claim'leri içeren ClaimsPrincipal içeren ServiceResult</returns>
    ServiceResult<ClaimsPrincipal> GetPrincipalFromExpiredToken(string accessToken);

    /// <summary>
    /// Refresh token kullanarak yeni bir access token oluşturur.
    /// Veritabanındaki refresh token'ı doğrular ve yeni token çifti döndürür.
    /// </summary>
    /// <param name="expiredAccessToken">Süresi dolmuş access token</param>
    /// <param name="refreshToken">Cookie'den okunan plain text refresh token</param>
    /// <returns>
    /// Yeni access token ve plain refresh token içeren RefreshTokenInternalResponse.
    /// Controller bu bilgiyi kullanarak access token'ı body'de, refresh token'ı cookie'de gönderir.
    /// </returns>
    Task<ServiceResult<RefreshTokenInternalResponse>> RefreshAccessTokenAsync(string expiredAccessToken, string refreshToken);

    /// <summary>
    /// Cookie'den refresh token okuyarak yeni bir access token oluşturur.
    /// Cookie okuma ve yazma işlemlerini servis içinde yapar.
    /// </summary>
    /// <param name="expiredAccessToken">Süresi dolmuş veya boş access token</param>
    /// <returns>
    /// Başarılı durumda yeni access token ve expiry bilgisi döner.
    /// Yeni refresh token otomatik olarak cookie'ye yazılır.
    /// </returns>
    Task<ServiceResult<RefreshTokenResponse>> RefreshAccessTokenWithCookieAsync(string expiredAccessToken);
}
