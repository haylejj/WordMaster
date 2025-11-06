using Core.Entity;
using Core.Requests;
using Core.Service;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Core.Results;


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
    public async Task<Result<UserEditViewModel>> GetUserEditViewModelAsync(string username)
    {
        var currentUser = await userManager.FindByNameAsync(username);

        return Result<UserEditViewModel>.Success(new UserEditViewModel()
        {
            UserName = currentUser.UserName,
            Email = currentUser.Email,
            Phone = currentUser.PhoneNumber,
            BirthDate = currentUser.BirthDate,
            City = currentUser.City,
            Gender = currentUser.Gender,
        });
    }
    public async Task<Result<IEnumerable<IdentityError>>> EditUserAsync(UserEditRequest request, string username)
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
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Güncelleme başarısız.", Data = updateResult.Errors };
        }
        await userManager.UpdateSecurityStampAsync(currentUser);
        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(currentUser, true);

        return Result<IEnumerable<IdentityError>>.Success(null);
    }
    public async Task<Result<bool>> CheckPasswordAsync(string userName, string passwordOld)
    {
        var currentUser = await userManager.FindByNameAsync(userName);
        var ok = await userManager.CheckPasswordAsync(currentUser!, passwordOld);
        return Result<bool>.Success(ok);
    }
    public async Task<Result<IEnumerable<IdentityError>>> ChangePasswordAsync(PasswordChangeRequest request, string userName)
    {
        var currentUser = await userManager.FindByNameAsync(userName);
        var resultChangePassword = await userManager.ChangePasswordAsync(currentUser, request.PasswordOld!, request.PasswordNew!);

        if (!resultChangePassword.Succeeded)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Şifre değiştirilemedi.", Data = resultChangePassword.Errors };
        }
        await userManager.UpdateSecurityStampAsync(currentUser);
        await signInManager.SignOutAsync();
        await signInManager.PasswordSignInAsync(currentUser, request.PasswordNew!, true, true);
        return Result<IEnumerable<IdentityError>>.Success(null);
    }
}
