using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : IUserService
{
    public async Task LogOutAsync() => await signInManager.SignOutAsync();

    public SelectList GetGenderSelectList() => new(Enum.GetNames<Gender>());

    public async Task<Result<UserEditViewModel>> GetUserEditViewModelAsync(string username)
    {
        var currentUser = await userManager.FindByNameAsync(username);

        if (currentUser == null)
        {
            return Result<UserEditViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        return Result<UserEditViewModel>.Success(new UserEditViewModel
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
        if (currentUser == null)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = [] };
        }

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
        if (currentUser == null)
        {
            return Result<bool>.Failure("Kullanıcı bulunamadı.");
        }

        var ok = await userManager.CheckPasswordAsync(currentUser, passwordOld);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IEnumerable<IdentityError>>> ChangePasswordAsync(PasswordChangeRequest request, string userName)
    {
        var currentUser = await userManager.FindByNameAsync(userName);
        if (currentUser == null)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = [] };
        }

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

    public async Task<List<UserViewModel>> GetUsersAsync()
    {
        var users = await userManager.Users.ToListAsync();

        return [.. users.Select(x => new UserViewModel { Id = x.Id, UserName = x.UserName!, Email = x.Email! })];
    }
}

