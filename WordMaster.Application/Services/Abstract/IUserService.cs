using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUserService
{
    Task LogOutAsync();
    SelectList GetGenderSelectList();
    Task<ServiceResult<UserEditViewModel>> GetUserEditViewModelAsync(string username);
    Task<ServiceResult<IEnumerable<IdentityError>>> EditUserAsync(UserEditRequest request, string username);
    Task<ServiceResult<bool>> CheckPasswordAsync(string userName, string passwordOld);
    Task<ServiceResult<IEnumerable<IdentityError>>> ChangePasswordAsync(PasswordChangeRequest request, string userName);
    Task<List<UserViewModel>> GetUsersAsync();
    Task<ServiceResult<(List<UserWithRolesViewModel> Users, int TotalCount)>> GetPagedUsersAsync(string? search, int page, int pageSize);
    Task<ServiceResult<UserWithRolesViewModel>> GetUserByIdAsync(string id);
    Task<ServiceResult<UserEditViewModel>> GetUserEditViewModelByIdAsync(string id);
    Task<ServiceResult<UserDetailViewModel>> GetUserDetailAsync(string id);
    Task<ServiceResult<IEnumerable<IdentityError>>> UpdateUserAsync(string id, UserEditRequest request);
    Task<ServiceResult<bool>> DeleteUserAsync(string id);
    Task<ServiceResult<string>> ResetUserPasswordAsync(string id);
}

