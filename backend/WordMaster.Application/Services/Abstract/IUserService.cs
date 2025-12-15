using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUserService
{
    /// <summary>
    /// Kullanıcının şifresini değiştirir.
    /// </summary>
    Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request, string userId);

    /// <summary>
    /// Tüm kullanıcıları listeler.
    /// </summary>
    Task<ServiceResult<List<UserResponse>>> GetUsersAsync();

    /// <summary>
    /// Kullanıcıları sayfalı olarak listeler.
    /// </summary>
    Task<ServiceResult<PagedResult<UserWithRolesResponse>>> GetPagedUsersAsync(string? search, int page, int pageSize);

    /// <summary>
    /// Kullanıcı profil bilgilerini ID ile getirir.
    /// </summary>
    Task<ServiceResult<UserProfileResponse>> GetProfileByIdAsync(string id);

    /// <summary>
    /// Kullanıcının detaylı bilgilerini getirir (istatistiklerle birlikte).
    /// </summary>
    Task<ServiceResult<UserDetailResponse>> GetUserDetailAsync(string id);

    /// <summary>
    /// Kullanıcı bilgilerini günceller.
    /// </summary>
    Task<ServiceResult> UpdateUserAsync(UserUpdateRequest request);

    /// <summary>
    /// Kullanıcıyı siler.
    /// </summary>
    Task<ServiceResult> DeleteUserAsync(string id);

    /// <summary>
    /// Kullanıcının şifresini sıfırlar ve yeni şifreyi e-posta ile gönderir.
    /// </summary>
    Task<ServiceResult<string>> ResetUserPasswordAsync(string id);

    /// <summary>
    /// Refresh token'ı doğrular ve kullanıcı bilgilerini döndürür.
    /// </summary>
    Task<ServiceResult<UserWithRolesResponse>> ValidateAndGetUserByRefreshTokenAsync(string userId, string refreshToken);

    /// <summary>
    /// Kullanıcının refresh token bilgilerini günceller.
    /// </summary>
    Task<ServiceResult> UpdateRefreshTokenAsync(string userId, string refreshToken, int expiresInDays);

    /// <summary>
    /// Kullanıcının rollerini değiştirir.
    /// </summary>
    Task<ServiceResult> ChangeUserRoleAsync(ChangeUserRoleRequest request);

    /// <summary>
    /// Refresh token ile kullanıcıyı bulur (sayfa yenileme durumu için).
    /// </summary>
    Task<ServiceResult<UserWithRolesResponse>> FindUserByRefreshTokenAsync(string refreshToken);
}
