using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Helpers;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class UserService(
    UserManager<AppUser> userManager,
    ILogHistoryService logHistoryService,
    IWordRepository wordRepository,
    IFavoriteRepository favoriteRepository,
    IUnknowsRepository unknowsRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<UserService> logger) : IUserService
{
    /// <summary>
    /// Kullanıcının şifresini değiştirir.
    /// </summary>
    public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request, string userId)
    {
        AppUser? currentUser = await userManager.FindByIdAsync(userId);
        if (currentUser == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        bool ok = await userManager.CheckPasswordAsync(currentUser, request.OldPassword!);
        if (!ok)
        {
            logger.LogWarning("Password change failed for user {UserId}. Incorrect old password.", userId);
            return ServiceResult.Failure("Mevcut şifre yanlış.", HttpStatusCode.BadRequest);
        }

        IdentityResult resultChangePassword = await userManager.ChangePasswordAsync(currentUser, request.OldPassword!, request.NewPassword!);
        if (!resultChangePassword.Succeeded)
        {
            List<string> errors = resultChangePassword.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Password change failed for user {UserId}. Errors: {Errors}", userId, string.Join(", ", errors));
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        await userManager.UpdateSecurityStampAsync(currentUser);
        await cacheService.RemoveAsync($"security_stamp:{currentUser.Id}");

        logger.LogInformation("Password changed successfully for user {UserId}.", userId);
        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tüm kullanıcıları listeler.
    /// </summary>
    public async Task<ServiceResult<List<UserResponse>>> GetUsersAsync()
    {
        List<AppUser> users = await userManager.Users.ToListAsync();
        List<UserResponse> userList = users.Select(x => new UserResponse
        {
            Id = x.Id.ToString(),
            UserName = x.UserName!,
            Email = x.Email!
        }).ToList();

        return ServiceResult<List<UserResponse>>.Success(userList, HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcıları sayfalı olarak listeler.
    /// </summary>
    public async Task<ServiceResult<PagedResult<UserWithRolesResponse>>> GetPagedUsersAsync(string? search, int page, int pageSize)
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
            .OrderBy(x => x.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<UserWithRolesResponse> userViewModels = new();
        foreach (AppUser user in users)
        {
            IList<string> userRoles = await userManager.GetRolesAsync(user);
            userViewModels.Add(new UserWithRolesResponse
            {
                Id = user.Id.ToString(),
                UserName = user.UserName!,
                Email = user.Email!,
                SecurityStamp = user.SecurityStamp,
                IsLockedOut = await userManager.IsLockedOutAsync(user),
                Gender = user.Gender,
                Roles = userRoles.ToList()
            });
        }

        PagedResult<UserWithRolesResponse> pagedResult = new()
        {
            Items = userViewModels,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<UserWithRolesResponse>>.Success(pagedResult, HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcı profil bilgilerini ID ile getirir.
    /// </summary>
    public async Task<ServiceResult<UserProfileResponse>> GetProfileByIdAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<UserProfileResponse>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        return ServiceResult<UserProfileResponse>.Success(new UserProfileResponse
        {
            UserName = user.UserName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            BirthDate = user.BirthDate,
            Gender = (int?)user.Gender,
        }, HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcının detaylı bilgilerini getirir (istatistiklerle birlikte).
    /// </summary>
    public async Task<ServiceResult<UserDetailResponse>> GetUserDetailAsync(string id)
    {
        if (!Guid.TryParse(id, out Guid userId))
        {
            return ServiceResult<UserDetailResponse>.Failure("Geçersiz ID formatı.", HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return ServiceResult<UserDetailResponse>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        UserLoginStatsResponse userLoginStats = await logHistoryService.GetUserLoginStatsAsync(id);
        LastLoginInfoResponse lastLoginInfo = await logHistoryService.GetLastSuccessfulLoginAsync(id);

        int wordCount = await wordRepository.CountAsync(x => x.UserId == userId);
        int favoriteCount = await favoriteRepository.CountAsync(x => x.UserId == userId);
        int unknowsCount = await unknowsRepository.CountAsync(x => x.UserId == userId);

        DateTime? lastPracticeDate = await wordRepository
            .Where(x => x.UserId == userId && x.LastPracticeDate != null)
            .OrderByDescending(x => x.LastPracticeDate)
            .Select(x => x.LastPracticeDate)
            .FirstOrDefaultAsync();

        UserDetailResponse detail = new()
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

        return ServiceResult<UserDetailResponse>.Success(detail, HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcı bilgilerini günceller.
    /// </summary>
    public async Task<ServiceResult> UpdateUserAsync(UserUpdateRequest request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            return ServiceResult.Failure("Kullanıcı ID'si zorunludur.", HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.FindByIdAsync(request.Id);
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
            logger.LogWarning("User update failed for user {UserId}. Errors: {Errors}", request.Id, string.Join(", ", errors));
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        logger.LogInformation("User profile updated successfully for user {UserId}.", request.Id);
        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Kullanıcıyı siler.
    /// </summary>
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

    /// <summary>
    /// Kullanıcının şifresini sıfırlar ve yeni şifreyi e-posta ile gönderir.
    /// </summary>
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

        string newPassword = PasswordHelper.GenerateRandomPassword();

        await unitOfWork.BeginTransactionAsync();
        try
        {
            IdentityResult removePasswordResult = await userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogError("Failed to remove password for user {UserId} during admin password reset", id);
                return ServiceResult<string>.Failure("Şifre sıfırlanırken bir hata oluştu.", HttpStatusCode.InternalServerError);
            }

            IdentityResult addPasswordResult = await userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogError("Failed to add new password for user {UserId} during admin password reset", id);
                return ServiceResult<string>.Failure("Yeni şifre atanırken bir hata oluştu.", HttpStatusCode.InternalServerError);
            }

            await unitOfWork.CommitAsync();

            ServiceResult emailResult = await emailService.SendPasswordToEmailAsync(newPassword, user.Email, user.UserName ?? "Kullanıcı");
            if (!emailResult.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogError("Password created but email failed to send for user {UserId}", id);
                return ServiceResult<string>.Failure($"Şifre oluşturuldu ancak email gönderilemedi: {emailResult.ErrorList?.FirstOrDefault() ?? "Bilinmeyen hata"}", HttpStatusCode.InternalServerError);
            }

            await unitOfWork.CommitTransactionAsync();
            await userManager.UpdateSecurityStampAsync(user);
            await cacheService.RemoveAsync($"security_stamp:{user.Id}");

            logger.LogInformation("Admin reset password successfully for user {UserId}", id);
            return ServiceResult<string>.Success(newPassword, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Exception occurred while resetting password for user {UserId}", id);
            return ServiceResult<string>.Failure($"Şifre sıfırlanırken bir hata oluştu: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Refresh token'ı doğrular ve kullanıcı bilgilerini döndürür.
    /// </summary>
    public async Task<ServiceResult<UserWithRolesResponse>> ValidateAndGetUserByRefreshTokenAsync(string userId, string refreshToken)
    {
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<UserWithRolesResponse>.Failure("Kullanıcı bulunamadı", HttpStatusCode.NotFound);
        }

        if (string.IsNullOrEmpty(user.RefreshToken) || !RefreshTokenHasher.VerifyRefreshToken(refreshToken, user.RefreshToken))
        {
            return ServiceResult<UserWithRolesResponse>.Failure("Geçersiz refresh token", HttpStatusCode.Unauthorized);
        }

        if (user.RefreshTokenExpires == null || user.RefreshTokenExpires < DateTime.UtcNow)
        {
            return ServiceResult<UserWithRolesResponse>.Failure("Refresh token süresi dolmuş", HttpStatusCode.Unauthorized);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        UserWithRolesResponse response = new()
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!,
            SecurityStamp = user.SecurityStamp,
            Gender = user.Gender,
            Roles = roles.ToList()
        };

        return ServiceResult<UserWithRolesResponse>.Success(response, HttpStatusCode.OK);
    }

    /// <summary>
    /// Kullanıcının refresh token bilgilerini günceller.
    /// </summary>
    public async Task<ServiceResult> UpdateRefreshTokenAsync(string userId, string refreshToken, int expiresInDays)
    {
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı", HttpStatusCode.NotFound);
        }

        user.RefreshToken = RefreshTokenHasher.HashRefreshToken(refreshToken);
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(expiresInDays);

        IdentityResult result = await userManager.UpdateAsync(user);
        return !result.Succeeded
            ? ServiceResult.Failure("Refresh token güncellenirken hata oluştu", HttpStatusCode.InternalServerError)
            : ServiceResult.Success(HttpStatusCode.NoContent);
    }
}
