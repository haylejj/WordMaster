using System.Security.Claims;
using WordMaster.Application.Responses;
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
    /// <param name="email">Kullanıcı email adresi</param>
    /// <param name="roles">Kullanıcının rolleri</param>
    /// <param name="securityStamp">Kullanıcının security stamp'i (şifre değişince token geçersiz olur)</param>
    /// <returns>JWT access token string içeren ServiceResult</returns>
    ServiceResult<string> GenerateAccessToken(string userId, string email, IList<string> roles, string securityStamp);

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
    /// <param name="refreshToken">Geçerli refresh token</param>
    /// <returns>Yeni access token ve refresh token içeren ServiceResult</returns>
    Task<ServiceResult<LoginResponse>> RefreshAccessTokenAsync(string expiredAccessToken, string refreshToken);
}
