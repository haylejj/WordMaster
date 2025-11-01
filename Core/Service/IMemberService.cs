using Core.Requests;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Core.Service
{
    public interface IMemberService
    {
        Task LogOutAsync();
        SelectList GetGenderSelectList();
        Task<UserEditViewModel> GetUserEditViewModelAsync(string username);
        Task<(bool, IEnumerable<IdentityError>?)> EditUserAsync(UserEditRequest request, string username);
        Task<bool> CheckPasswordAsync(string userName, string passwordOld);
        Task<(bool, IEnumerable<IdentityError>?)> ChangePasswordAsync(PasswordChangeRequest request, string userName);
    }
}
