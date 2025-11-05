using Core.Entity;
using Core.Requests;
using Core.Service;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Service.Service;

public class MemberService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : IMemberService
{
    public async Task LogOutAsync()
    {
        await signInManager.SignOutAsync();
    }
    public SelectList GetGenderSelectList()
    {
        return new SelectList(Enum.GetNames(typeof(Gender)));
    }
    public async Task<UserEditViewModel> GetUserEditViewModelAsync(string username)
    {
        var currentUser = await userManager.FindByNameAsync(username);

        return new UserEditViewModel()
        {
            UserName = currentUser.UserName,
            Email = currentUser.Email,
            Phone = currentUser.PhoneNumber,
            BirthDate = currentUser.BirthDate,
            City = currentUser.City,
            Gender = currentUser.Gender,
        };
    }
    public async Task<(bool, IEnumerable<IdentityError>?)> EditUserAsync(UserEditRequest request, string username)
    {
        var currentUser = await userManager.FindByNameAsync(username);

        currentUser.UserName = request.UserName;
        currentUser.Email = request.Email;
        currentUser.PhoneNumber = request.Phone;
        currentUser.BirthDate = request.BirthDate;
        currentUser.City = request.City;
        currentUser.Gender = request.Gender;

        var updateResult = await userManager.UpdateAsync(currentUser);
        if (!updateResult.Succeeded)
        {
            return (false, updateResult.Errors);
        }
        await userManager.UpdateSecurityStampAsync(currentUser);
        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(currentUser, true);

        return (true, null);
    }
    public async Task<bool> CheckPasswordAsync(string userName, string passwordOld)
    {
        var currentUser = await userManager.FindByNameAsync(userName);

        return await userManager.CheckPasswordAsync(currentUser!, passwordOld);
    }
    public async Task<(bool, IEnumerable<IdentityError>?)> ChangePasswordAsync(PasswordChangeRequest request, string userName)
    {
        var currentUser = await userManager.FindByNameAsync(userName);
        var resultChangePassword = await userManager.ChangePasswordAsync(currentUser, request.PasswordOld!, request.PasswordNew!);

        if (!resultChangePassword.Succeeded)
        {
            return (false, resultChangePassword.Errors);
        }
        await userManager.UpdateSecurityStampAsync(currentUser);
        await signInManager.SignOutAsync();
        await signInManager.PasswordSignInAsync(currentUser, request.PasswordNew!, true, true);
        return (true, null);
    }
}
