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

    public async Task<ServiceResult<UserEditViewModel>> GetUserEditViewModelAsync(string username)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(username);

        return currentUser == null
            ? ServiceResult<UserEditViewModel>.Failure("Kullanıcı bulunamadı.")
            : ServiceResult<UserEditViewModel>.Success(new UserEditViewModel
            {
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                Phone = currentUser.PhoneNumber,
                BirthDate = currentUser.BirthDate,
                Gender = currentUser.Gender,
            });
    }

    public async Task<ServiceResult<IEnumerable<IdentityError>>> EditUserAsync(UserEditRequest request, string username)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(username);
        if (currentUser == null)
        {
            return new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = [] };
        }

        currentUser.UserName = request.UserName;
        currentUser.Email = request.Email;
        currentUser.PhoneNumber = request.Phone;
        currentUser.BirthDate = request.BirthDate;
        currentUser.Gender = request.Gender;
        // Removed Picture assignment since it's being removed from the form

        IdentityResult updateResult = await userManager.UpdateAsync(currentUser);
        if (!updateResult.Succeeded)
        {
            return new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Güncelleme başarısız.", Data = updateResult.Errors };
        }

        await userManager.UpdateSecurityStampAsync(currentUser);
        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(currentUser, true);

        return ServiceResult<IEnumerable<IdentityError>>.Success(null);
    }

    public async Task<ServiceResult<bool>> CheckPasswordAsync(string userName, string passwordOld)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(userName);
        if (currentUser == null)
        {
            return ServiceResult<bool>.Failure("Kullanıcı bulunamadı.");
        }

        bool ok = await userManager.CheckPasswordAsync(currentUser, passwordOld);
        return ServiceResult<bool>.Success(ok);
    }

    public async Task<ServiceResult<IEnumerable<IdentityError>>> ChangePasswordAsync(PasswordChangeRequest request, string userName)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(userName);
        if (currentUser == null)
        {
            return new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = [] };
        }

        IdentityResult resultChangePassword = await userManager.ChangePasswordAsync(currentUser, request.PasswordOld!, request.PasswordNew!);

        if (!resultChangePassword.Succeeded)
        {
            return new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Şifre değiştirilemedi.", Data = resultChangePassword.Errors };
        }

        await userManager.UpdateSecurityStampAsync(currentUser);
        await signInManager.SignOutAsync();
        await signInManager.PasswordSignInAsync(currentUser, request.PasswordNew!, true, true);
        return ServiceResult<IEnumerable<IdentityError>>.Success(null);
    }

    public async Task<List<UserViewModel>> GetUsersAsync()
    {
        List<AppUser> users = await userManager.Users.ToListAsync();

        return [.. users.Select(x => new UserViewModel { Id = x.Id.ToString(), UserName = x.UserName!, Email = x.Email! })];
    }

    public async Task<ServiceResult<(List<UserWithRolesViewModel> Users, int TotalCount)>> GetPagedUsersAsync(string? search, int page, int pageSize)
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
            // Note: ToString() in query might not be supported by all providers or might cause client evaluation
            query = query.Where(x => x.UserName!.Contains(search) || x.Email!.Contains(search) || x.Id.ToString().Contains(search));
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
                Id = user.Id.ToString(),
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = [.. roles]
            });
        }

        return ServiceResult<(List<UserWithRolesViewModel> Users, int TotalCount)>.Success((usersWithRoles, totalCount));
    }

    public async Task<ServiceResult<UserWithRolesViewModel>> GetUserByIdAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserWithRolesViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        UserWithRolesViewModel userWithRoles = new()
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = [.. roles]
        };

        return ServiceResult<UserWithRolesViewModel>.Success(userWithRoles);
    }

    public async Task<ServiceResult<UserEditViewModel>> GetUserEditViewModelByIdAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        return user == null
            ? ServiceResult<UserEditViewModel>.Failure("Kullanıcı bulunamadı.")
            : ServiceResult<UserEditViewModel>.Success(new UserEditViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
            });
    }

    public async Task<ServiceResult<UserDetailViewModel>> GetUserDetailAsync(string id)
    {
        if (!Guid.TryParse(id, out Guid userId))
        {
            return ServiceResult<UserDetailViewModel>.Failure("Geçersiz ID formatı.");
        }

        AppUser? user = await userManager.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return ServiceResult<UserDetailViewModel>.Failure("Kullanıcı bulunamadı.");
        }

        // Login Statistics
        UserLoginStatsViewModel userLoginStats = await logHistoryService.GetUserLoginStatsAsync(id);
        LastLoginInfoViewModel lastLoginInfo = await logHistoryService.GetLastSuccessfulLoginAsync(id);

        // Word Statistics
        int wordCount = await wordRepository.CountAsync(x => x.UserId == userId);
        int favoriteCount = await favoriteRepository.CountAsync(x => x.UserId == userId);
        int unknowsCount = await unknowsRepository.CountAsync(x => x.UserId == userId);

        DateTime? lastPracticeDate = await wordRepository
            .Where(x => x.UserId == userId && x.LastPracticeDate != null)
            .OrderByDescending(x => x.LastPracticeDate)
            .Select(x => x.LastPracticeDate)
            .FirstOrDefaultAsync();

        UserDetailViewModel detail = new()
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!,
            Phone = user.PhoneNumber,
            BirthDate = user.BirthDate,
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

        return ServiceResult<UserDetailViewModel>.Success(detail);
    }

    public async Task<ServiceResult<IEnumerable<IdentityError>>> UpdateUserAsync(string id, UserEditRequest request)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = [] };
        }

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.PhoneNumber = request.Phone;
        user.BirthDate = request.BirthDate;
        user.Gender = request.Gender;

        IdentityResult updateResult = await userManager.UpdateAsync(user);
        return !updateResult.Succeeded
            ? new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Güncelleme başarısız.", Data = updateResult.Errors }
            : ServiceResult<IEnumerable<IdentityError>>.Success(null);
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<bool>.Failure("Kullanıcı bulunamadı.");
        }

        IdentityResult result = await userManager.DeleteAsync(user);
        return !result.Succeeded ? ServiceResult<bool>.Failure("Kullanıcı silinirken bir hata oluştu.") : ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<string>> ResetUserPasswordAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<string>.Failure("Kullanıcı bulunamadı.");
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            return ServiceResult<string>.Failure("Kullanıcının email adresi bulunamadı.");
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
                return ServiceResult<string>.Failure("Şifre sıfırlanırken bir hata oluştu.");
            }

            IdentityResult addPasswordResult = await userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<string>.Failure("Yeni şifre atanırken bir hata oluştu.");
            }

            // Değişiklikleri kaydet
            await unitOfWork.CommitAsync();

            // Şifreyi email olarak gönder
            await emailService.SendPasswordToEmailAsync(newPassword, user.Email, user.UserName ?? "Kullanıcı");

            // Email başarılı oldu, transaction'ı commit et
            await unitOfWork.CommitTransactionAsync();

            return ServiceResult<string>.Success(newPassword);
        }
        catch (Exception ex)
        {
            // Hata durumunda transaction'ı rollback et
            await unitOfWork.RollbackTransactionAsync();
            return ServiceResult<string>.Failure($"Şifre sıfırlanırken bir hata oluştu: {ex.Message}");
        }
    }
}

