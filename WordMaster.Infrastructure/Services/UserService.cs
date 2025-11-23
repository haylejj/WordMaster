using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses;
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
            ? ServiceResult<UserEditViewModel>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound)
            : ServiceResult<UserEditViewModel>.Success(new UserEditViewModel
            {
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                Phone = currentUser.PhoneNumber,
                BirthDate = currentUser.BirthDate,
                Gender = currentUser.Gender,
            }, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> EditUserAsync(UserEditRequest request, string username)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(username);
        if (currentUser == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
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
            List<string> errors = updateResult.Errors.Select(e => e.Description).ToList();
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        await userManager.UpdateSecurityStampAsync(currentUser);
        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(currentUser, true);

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult<bool>> CheckPasswordAsync(string userName, string passwordOld)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(userName);
        if (currentUser == null)
        {
            return ServiceResult<bool>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        bool ok = await userManager.CheckPasswordAsync(currentUser, passwordOld);
        return ServiceResult<bool>.Success(ok, HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcının şifresini değiştirir.
    /// </summary>
    /// <param name="request">Eski ve yeni şifre bilgilerini içeren istek.</param>
    /// <param name="userName">Şifresi değiştirilecek kullanıcının kullanıcı adı.</param>
    /// <returns>İşlem sonucunu döner.</returns>
    public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request, string userName)
    {
        AppUser? currentUser = await userManager.FindByNameAsync(userName);
        if (currentUser == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }
        bool ok = await userManager.CheckPasswordAsync(currentUser, request.PasswordOld!);
        if (!ok)
        {
            return ServiceResult.Failure("Mevcut şifre yanlış.", HttpStatusCode.BadRequest);
        }

        IdentityResult resultChangePassword = await userManager.ChangePasswordAsync(currentUser, request.PasswordOld!, request.PasswordNew!);

        if (!resultChangePassword.Succeeded)
        {
            List<string> errors = resultChangePassword.Errors.Select(e => e.Description).ToList();
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        // Security Stamp'i güncelle (bu işlem mevcut JWT token'ları geçersiz kılar)
        await userManager.UpdateSecurityStampAsync(currentUser);

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<List<UserViewModel>> GetUsersAsync()
    {
        List<AppUser> users = await userManager.Users.ToListAsync();

        return [.. users.Select(x => new UserViewModel { Id = x.Id.ToString(), UserName = x.UserName!, Email = x.Email! })];
    }

    public async Task<ServiceResult<PagedResult<UserWithRolesViewModel>>> GetPagedUsersAsync(string? search, int page, int pageSize)
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
            query = query.Where(x => x.UserName!.Contains(search) || x.Email!.Contains(search));
        }

        int totalCount = await query.CountAsync();

        List<AppUser> users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<UserWithRolesViewModel> userViewModels = new();

        foreach (AppUser user in users)
        {
            IList<string> userRoles = await userManager.GetRolesAsync(user);

            userViewModels.Add(new UserWithRolesViewModel
            {
                Id = user.Id.ToString(),
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = userRoles.ToList()
            });
        }

        PagedResult<UserWithRolesViewModel> pagedResult = new()
        {
            Items = userViewModels,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<UserWithRolesViewModel>>.Success(pagedResult, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UserWithRolesViewModel>> GetUserByIdAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserWithRolesViewModel>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        UserWithRolesViewModel userWithRoles = new()
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = [.. roles]
        };

        return ServiceResult<UserWithRolesViewModel>.Success(userWithRoles, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UserEditViewModel>> GetUserEditViewModelByIdAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        return user == null
            ? ServiceResult<UserEditViewModel>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound)
            : ServiceResult<UserEditViewModel>.Success(new UserEditViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
            }, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UserDetailViewModel>> GetUserDetailAsync(string id)
    {
        if (!Guid.TryParse(id, out Guid userId))
        {
            return ServiceResult<UserDetailViewModel>.Failure("Geçersiz ID formatı.", HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return ServiceResult<UserDetailViewModel>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
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

        return ServiceResult<UserDetailViewModel>.Success(detail, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> UpdateUserAsync(string id, UserEditRequest request)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.PhoneNumber = request.Phone;
        user.BirthDate = request.BirthDate;
        user.Gender = request.Gender;

        IdentityResult updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            List<string> errors = updateResult.Errors.Select(e => e.Description).ToList();
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult> DeleteUserAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        IdentityResult result = await userManager.DeleteAsync(user);
        return !result.Succeeded
            ? ServiceResult.Failure("Kullanıcı silinirken bir hata oluştu.", HttpStatusCode.InternalServerError)
            : ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult<string>> ResetUserPasswordAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<string>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            return ServiceResult<string>.Failure("Kullanıcının email adresi bulunamadı.", HttpStatusCode.BadRequest);
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
                return ServiceResult<string>.Failure("Şifre sıfırlanırken bir hata oluştu.", HttpStatusCode.InternalServerError);
            }

            IdentityResult addPasswordResult = await userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<string>.Failure("Yeni şifre atanırken bir hata oluştu.", HttpStatusCode.InternalServerError);
            }

            // Değişiklikleri kaydet
            await unitOfWork.CommitAsync();

            // Şifreyi email olarak gönder
            ServiceResult emailResult = await emailService.SendPasswordToEmailAsync(newPassword, user.Email, user.UserName ?? "Kullanıcı");

            if (!emailResult.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ServiceResult<string>.Failure($"Şifre oluşturuldu ancak email gönderilemedi: {emailResult.ErrorList?.FirstOrDefault() ?? "Bilinmeyen hata"}", HttpStatusCode.InternalServerError);
            }

            // Email başarılı oldu, transaction'ı commit et
            await unitOfWork.CommitTransactionAsync();

            return ServiceResult<string>.Success(newPassword, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            // Hata durumunda transaction'ı rollback et
            await unitOfWork.RollbackTransactionAsync();
            return ServiceResult<string>.Failure($"Şifre sıfırlanırken bir hata oluştu: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResult<UserWithRolesResponse>> ValidateAndGetUserByRefreshTokenAsync(string userId, string refreshToken)
    {
        // Kullanıcıyı veritabanından getir
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<UserWithRolesResponse>.Failure("Kullanıcı bulunamadı", HttpStatusCode.NotFound);
        }

        // Veritabanındaki refresh token hash'i ile gelen refresh token'ı karşılaştır
        if (string.IsNullOrEmpty(user.RefreshToken) || !RefreshTokenHasher.VerifyRefreshToken(refreshToken, user.RefreshToken))
        {
            return ServiceResult<UserWithRolesResponse>.Failure("Geçersiz refresh token", HttpStatusCode.Unauthorized);
        }

        // Refresh token'ın süresinin dolup dolmadığını kontrol et
        if (user.RefreshTokenExpires == null || user.RefreshTokenExpires < DateTime.UtcNow)
        {
            return ServiceResult<UserWithRolesResponse>.Failure("Refresh token süresi dolmuş", HttpStatusCode.Unauthorized);
        }

        // Kullanıcının rollerini al
        IList<string> roles = await userManager.GetRolesAsync(user);

        UserWithRolesResponse response = new()
        {
            User = user,
            Roles = roles
        };

        return ServiceResult<UserWithRolesResponse>.Success(response, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> UpdateRefreshTokenAsync(string userId, string refreshToken, int expiresInDays)
    {
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı", HttpStatusCode.NotFound);
        }

        // Refresh token'ı hash'leyerek sakla (güvenlik için)
        user.RefreshToken = RefreshTokenHasher.HashRefreshToken(refreshToken);
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(expiresInDays);

        IdentityResult result = await userManager.UpdateAsync(user);
        return !result.Succeeded
            ? ServiceResult.Failure("Refresh token güncellenirken hata oluştu", HttpStatusCode.InternalServerError)
            : ServiceResult.Success(HttpStatusCode.NoContent);
    }
}
