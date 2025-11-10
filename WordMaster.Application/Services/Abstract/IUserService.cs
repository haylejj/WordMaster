using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using WordMaster.Application.Requests;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUserService
{
    Task LogOutAsync();
    SelectList GetGenderSelectList();
    Task<Result<UserEditViewModel>> GetUserEditViewModelAsync(string username);
    Task<Result<IEnumerable<IdentityError>>> EditUserAsync(UserEditRequest request, string username);
    Task<Result<bool>> CheckPasswordAsync(string userName, string passwordOld);
    Task<Result<IEnumerable<IdentityError>>> ChangePasswordAsync(PasswordChangeRequest request, string userName);
    Task<List<UserViewModel>> GetUsersAsync();
    Task<Result<(List<UserWithRolesViewModel> Users, int TotalCount)>> GetPagedUsersAsync(string? search, int page, int pageSize);
    Task<Result<UserWithRolesViewModel>> GetUserByIdAsync(string id);
    Task<Result<UserEditViewModel>> GetUserEditViewModelByIdAsync(string id);
    Task<Result<UserDetailViewModel>> GetUserDetailAsync(string id);
    Task<Result<IEnumerable<IdentityError>>> UpdateUserAsync(string id, UserEditRequest request);
    Task<Result<bool>> DeleteUserAsync(string id);
}

