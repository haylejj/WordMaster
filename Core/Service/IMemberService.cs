using Core.Requests;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Core.Results;

namespace Core.Service;

public interface IMemberService
{
    Task LogOutAsync();
    SelectList GetGenderSelectList();
    Task<Result<UserEditViewModel>> GetUserEditViewModelAsync(string username);
    Task<Result<IEnumerable<IdentityError>>> EditUserAsync(UserEditRequest request, string username);
    Task<Result<bool>> CheckPasswordAsync(string userName, string passwordOld);
    Task<Result<IEnumerable<IdentityError>>> ChangePasswordAsync(PasswordChangeRequest request, string userName);
}
