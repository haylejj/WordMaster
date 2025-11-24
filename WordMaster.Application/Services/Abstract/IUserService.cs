using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUserService
{
    Task LogOutAsync();
    SelectList GetGenderSelectList();
    Task<ServiceResult<UserEditViewModel>> GetUserEditViewModelAsync(string username);
    Task<ServiceResult> EditUserAsync(UserEditRequest request, string username);
    Task<ServiceResult<bool>> CheckPasswordAsync(string userName, string passwordOld);
    Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request, string userName);
    Task<ServiceResult<List<UserViewModel>>> GetUsersAsync();
    Task<ServiceResult<PagedResult<UserWithRolesViewModel>>> GetPagedUsersAsync(string? search, int page, int pageSize);
    Task<ServiceResult<UserWithRolesViewModel>> GetUserByIdAsync(string id);
    Task<ServiceResult<UserEditViewModel>> GetUserEditViewModelByIdAsync(string id);
    Task<ServiceResult<UserDetailViewModel>> GetUserDetailAsync(string id);
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
