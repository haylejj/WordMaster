using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Admin;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Helpers;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogHistoryService logHistoryService, IWordRepository wordRepository, IFavoriteRepository favoriteRepository, IUnknowsRepository unknowsRepository, IEmailService emailService, IUnitOfWork unitOfWork) : IUserService
{
    public async Task LogOutAsync()
    {
        await signInManager.SignOutAsync();
    }

    public SelectList GetGenderSelectList()
    {
        return new(Enum.GetNames<Gender>());
    }

    public async Task<Result<UserEditViewModel>> GetUserEditViewModelAsync(string username)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(username);

        return currentUser == null
            ? Result<UserEditViewModel>.Failure("Kullanıcı bulunamadı.")
            : Result<UserEditViewModel>.Success(new UserEditViewModel
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
        AppUser? currentUser = await userManager.FindByNameAsync(username);
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

        IdentityResult updateResult = await userManager.UpdateAsync(currentUser);
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
        AppUser? currentUser = await userManager.FindByNameAsync(userName);
        if (currentUser == null)
        {
            return Result<bool>.Failure("Kullanıcı bulunamadı.");
        }

        bool ok = await userManager.CheckPasswordAsync(currentUser, passwordOld);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IEnumerable<IdentityError>>> ChangePasswordAsync(PasswordChangeRequest request, string userName)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(userName);
        if (currentUser == null)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = [] };
        }

        IdentityResult resultChangePassword = await userManager.ChangePasswordAsync(currentUser, request.PasswordOld!, request.PasswordNew!);

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
        List<AppUser> users = await userManager.Users.ToListAsync();

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

        IQueryable<AppUser> query = userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.UserName!.Contains(search) || x.Email!.Contains(search) || x.Id.Contains(search));
        }

        int totalCount = await query.CountAsync();

        List<AppUser> users = await query
            .OrderBy(x => x.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<UserWithRolesViewModel> usersWithRoles = new();

        foreach (AppUser? user in users)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRolesViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = [.. roles]
            });
        }

        return Result<(List<UserWithRolesViewModel> Users, int TotalCount)>.Success((usersWithRoles, totalCount));
    }

    public async Task<Result<UserWithRolesViewModel>> GetUserByIdAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<UserWithRolesViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        var userWithRoles = new UserWithRolesViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = [.. roles]
        };

        return Result<UserWithRolesViewModel>.Success(userWithRoles);
    }

    public async Task<Result<UserEditViewModel>> GetUserEditViewModelByIdAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        return user == null
            ? Result<UserEditViewModel>.Failure("Kullanıcı bulunamadı.")
            : Result<UserEditViewModel>.Success(new UserEditViewModel
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
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<UserDetailViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        // Login Statistics
        UserLoginStatsViewModel userLoginStats = await logHistoryService.GetUserLoginStatsAsync(id);
        LastLoginInfoViewModel lastLoginInfo = await logHistoryService.GetLastSuccessfulLoginAsync(id);

        // Word Statistics
        int wordCount = await wordRepository.CountAsync(x => x.UserId == id);
        int favoriteCount = await favoriteRepository.CountAsync(x => x.UserId == id);
        int unknowsCount = await unknowsRepository.CountAsync(x => x.UserId == id);

        DateTime? lastPracticeDate = await wordRepository
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
            TotalLoginAttempts = userLoginStats.TotalLogins,
            SuccessfulLogins = userLoginStats.SuccessfulLogins,
            FailedLogins = userLoginStats.FailedLogins,
            LastLoginDate = lastLoginInfo.LastLoginDate,
            LastLoginIpAddress = lastLoginInfo.LastLoginIpAddress,
            WordCount = wordCount,
            FavoriteCount = favoriteCount,
            UnknowsCount = unknowsCount,
            LastPracticeDate = lastPracticeDate
        };

        return Result<UserDetailViewModel>.Success(detail);
    }

    public async Task<Result<IEnumerable<IdentityError>>> UpdateUserAsync(string id, UserEditRequest request)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
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

        IdentityResult updateResult = await userManager.UpdateAsync(user);
        return !updateResult.Succeeded
            ? new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Güncelleme başarısız.", Data = updateResult.Errors }
            : Result<IEnumerable<IdentityError>>.Success(null);
    }

    public async Task<Result<bool>> DeleteUserAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<bool>.Failure("Kullanıcı bulunamadı.");
        }

        IdentityResult result = await userManager.DeleteAsync(user);
        return !result.Succeeded ? Result<bool>.Failure("Kullanıcı silinirken bir hata oluştu.") : Result<bool>.Success(true);
    }

    public async Task<Result<string>> ResetUserPasswordAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı.");
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            return Result<string>.Failure("Kullanıcının email adresi bulunamadı.");
        }

        // Kriptografik olarak güvenli rastgele şifre oluştur
        string newPassword = PasswordHelper.GenerateRandomPassword();

        // Transaction başlat
        await unitOfWork.BeginTransactionAsync();
        try
        {
            // Mevcut şifreyi kaldır ve yeni şifreyi ayarla
            IdentityResult removePasswordResult = await userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                await unitOfWork.RollbackTransactionAsync();
                return Result<string>.Failure("Şifre sıfırlanırken bir hata oluştu.");
            }

            IdentityResult addPasswordResult = await userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                await unitOfWork.RollbackTransactionAsync();
                return Result<string>.Failure("Yeni şifre atanırken bir hata oluştu.");
            }

            // Değişiklikleri kaydet
            await unitOfWork.CommitAsync();

            // Şifreyi email olarak gönder
            await emailService.SendPasswordToEmailAsync(newPassword, user.Email, user.UserName ?? "Kullanıcı");

            // Email başarılı oldu, transaction'ı commit et
            await unitOfWork.CommitTransactionAsync();

            return Result<string>.Success(newPassword);
        }
        catch (Exception ex)
        {
            // Hata durumunda transaction'ı rollback et
            await unitOfWork.RollbackTransactionAsync();
            return Result<string>.Failure($"Şifre sıfırlanırken bir hata oluştu: {ex.Message}");
        }
    }
}

