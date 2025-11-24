using Microsoft.AspNetCore.Mvc.Rendering;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUserService
{
    Task LogOutAsync();
    SelectList GetGenderSelectList();
    Task<ServiceResult<UserEditResponse>> GetUserEditViewModelAsync(string username);
    Task<ServiceResult> EditUserAsync(UserEditRequest request, string username);
    Task<ServiceResult<bool>> CheckPasswordAsync(string userName, string passwordOld);
    Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request, string userName);
    Task<ServiceResult<List<UserResponse>>> GetUsersAsync();
    Task<ServiceResult<PagedResult<UserWithRolesResponse>>> GetPagedUsersAsync(string? search, int page, int pageSize);
    Task<ServiceResult<UserWithRolesResponse>> GetUserByIdAsync(string id);
    Task<ServiceResult<UserEditResponse>> GetUserEditViewModelByIdAsync(string id);
    Task<ServiceResult<UserDetailResponse>> GetUserDetailAsync(string id);
    Task<ServiceResult> UpdateUserAsync(UserUpdateRequest request);
    Task<ServiceResult> DeleteUserAsync(string id);
    Task<ServiceResult<string>> ResetUserPasswordAsync(string id);

    /// <summary>
    /// Refresh token'ı doğrular ve kullanıcı bilgilerini döndürür.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="refreshToken">Doğrulanacak refresh token</param>
    /// <returns>Kullanıcı ve rolleri içeren ServiceResult</returns>
    Task<ServiceResult<UserWithRolesResponse>> ValidateAndGetUserByRefreshTokenAsync(string userId, string refreshToken);

    /// <summary>
    /// Kullanıcının refresh token bilgilerini günceller.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="refreshToken">Yeni refresh token</param>
    /// <param name="expiresInDays">Geçerlilik süresi (gün)</param>
    Task<ServiceResult> UpdateRefreshTokenAsync(string userId, string refreshToken, int expiresInDays);
}
