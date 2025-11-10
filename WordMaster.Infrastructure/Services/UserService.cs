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
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.Services;

public class UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, AppDbContext context, ICacheService cacheService) : IUserService
{
    private readonly ICacheService _cacheService = cacheService;
    private static readonly TimeSpan PagedUsersCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan UserDetailCacheExpiration = TimeSpan.FromMinutes(10);
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

    public async Task<Result<(List<UserWithRolesViewModel> Users, int TotalCount)>> GetPagedUsersAsync(string? search, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        var query = userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.UserName!.Contains(search) || x.Email!.Contains(search) || x.Id.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(x => x.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var usersWithRoles = new List<UserWithRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRolesViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList()
            });
        }

        return Result<(List<UserWithRolesViewModel> Users, int TotalCount)>.Success((usersWithRoles, totalCount));
    }

    public async Task<Result<UserWithRolesViewModel>> GetUserByIdAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<UserWithRolesViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var userWithRoles = new UserWithRolesViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return Result<UserWithRolesViewModel>.Success(userWithRoles);
    }

    public async Task<Result<UserEditViewModel>> GetUserEditViewModelByIdAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<UserEditViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        return Result<UserEditViewModel>.Success(new UserEditViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            BirthDate = user.BirthDate,
            City = user.City,
            Gender = user.Gender,
        });
    }

    public async Task<Result<UserDetailViewModel>> GetUserDetailAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<UserDetailViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        // Login Statistics
        var loginStats = await context.LogHistories
            .Where(x => x.AppUserId == id)
            .GroupBy(x => x.IsSuccessful)
            .Select(g => new { IsSuccessful = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalLogins = await context.LogHistories.CountAsync(x => x.AppUserId == id);
        var successfulLogins = loginStats.FirstOrDefault(x => x.IsSuccessful)?.Count ?? 0;
        var failedLogins = loginStats.FirstOrDefault(x => !x.IsSuccessful)?.Count ?? 0;

        var lastLogin = await context.LogHistories
            .Where(x => x.AppUserId == id && x.IsSuccessful)
            .OrderByDescending(x => x.AttemptedAt)
            .Select(x => x.AttemptedAt)
            .FirstOrDefaultAsync();

        // Word Statistics
        var wordCount = await context.Words.CountAsync(x => x.UserId == id);
        var favoriteCount = await context.Favorites.CountAsync(x => x.UserId == id);
        var unknowsCount = await context.Unknows.CountAsync(x => x.UserId == id);

        var lastPracticeDate = await context.Words
            .Where(x => x.UserId == id && x.LastPracticeDate != null)
            .OrderByDescending(x => x.LastPracticeDate)
            .Select(x => x.LastPracticeDate)
            .FirstOrDefaultAsync();

        var detail = new UserDetailViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Phone = user.PhoneNumber,
            BirthDate = user.BirthDate,
            City = user.City,
            Gender = user.Gender,
            TotalLoginAttempts = totalLogins,
            SuccessfulLogins = successfulLogins,
            FailedLogins = failedLogins,
            LastLoginDate = lastLogin != default ? lastLogin : null,
            WordCount = wordCount,
            FavoriteCount = favoriteCount,
            UnknowsCount = unknowsCount,
            LastPracticeDate = lastPracticeDate
        };

        return Result<UserDetailViewModel>.Success(detail);
    }

    public async Task<Result<IEnumerable<IdentityError>>> UpdateUserAsync(string id, UserEditRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = [] };
        }

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.PhoneNumber = request.Phone;
        user.BirthDate = request.BirthDate;
        user.City = request.City;
        user.Gender = request.Gender;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Güncelleme başarısız.", Data = updateResult.Errors };
        }

        return Result<IEnumerable<IdentityError>>.Success(null);
    }

    public async Task<Result<bool>> DeleteUserAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<bool>.Failure("Kullanıcı bulunamadı.");
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return Result<bool>.Failure("Kullanıcı silinirken bir hata oluştu.");
        }

        return Result<bool>.Success(true);
    }
}

