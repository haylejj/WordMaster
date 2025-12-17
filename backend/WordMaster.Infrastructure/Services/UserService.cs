using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.Services;

public class UserService(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    AppDbContext dbContext,
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

        // N+1 Query Çözümü: Tüm kullanıcıların ID'lerini al ve tek sorguda rolleri getir
        List<Guid> userIds = users.Select(u => u.Id).ToList();

        // UserRoles ve Roles tablolarını join ederek tek sorguda tüm rolleri al
        Dictionary<Guid, List<string>> userRolesDict = await dbContext.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(
                roleManager.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, RoleName = r.Name! })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(x => x.RoleName).ToList());

        // Lockout durumlarını kontrol et (bu da batch'lenebilir ama basit tutalım)
        DateTime now = DateTime.UtcNow;

        List<UserWithRolesResponse> userViewModels = users.Select(user => new UserWithRolesResponse
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!,
            SecurityStamp = user.SecurityStamp,
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > now,
            Roles = userRolesDict.TryGetValue(user.Id, out List<string>? roles) ? roles : []
        }).ToList();

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
            FirstName = user.FirstName,
            LastName = user.LastName
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
            FirstName = user.FirstName,
            LastName = user.LastName,
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
            logger.LogWarning("User update failed. User ID is missing.");
            return ServiceResult.Failure("Kullanıcı ID'si zorunludur.", HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.FindByIdAsync(request.Id);
        if (user == null)
        {
            logger.LogWarning("User update failed. User not found: {UserId}", request.Id);
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        user.Email = request.Email;
        user.PhoneNumber = request.Phone;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

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
            logger.LogWarning("User deletion failed. User not found: {UserId}", id);
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        IdentityResult result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("User deletion failed for {UserId}. Changes not succeeded.", id);
            return ServiceResult.Failure("Kullanıcı silinirken bir hata oluştu.", HttpStatusCode.InternalServerError);
        }

        logger.LogInformation("User deleted successfully: {UserId}", id);
        return ServiceResult.Success(HttpStatusCode.NoContent);
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

        // Use execution strategy to handle SqlServerRetryingExecutionStrategy with transactions
        IExecutionStrategy strategy = unitOfWork.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await unitOfWork.BeginTransactionAsync();

                IdentityResult removePasswordResult = await userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("REMOVE_PASSWORD_FAILED");
                }

                IdentityResult addPasswordResult = await userManager.AddPasswordAsync(user, newPassword);
                if (!addPasswordResult.Succeeded)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("ADD_PASSWORD_FAILED");
                }

                await unitOfWork.CommitAsync();

                ServiceResult emailResult = await emailService.SendPasswordToEmailAsync(newPassword, user.Email, user.UserName ?? "Kullanıcı");
                if (!emailResult.IsSuccess)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException($"EMAIL_FAILED:{emailResult.ErrorList?.FirstOrDefault() ?? "Bilinmeyen hata"}");
                }

                await unitOfWork.CommitTransactionAsync();
            });

            await userManager.UpdateSecurityStampAsync(user);
            await cacheService.RemoveAsync($"security_stamp:{user.Id}");

            logger.LogInformation("Admin reset password successfully for user {UserId}. New password email sent.", id);
            return ServiceResult<string>.Success(newPassword, HttpStatusCode.OK);
        }
        catch (InvalidOperationException ex) when (ex.Message == "REMOVE_PASSWORD_FAILED")
        {
            logger.LogError("Failed to remove password for user {UserId} during admin password reset", id);
            return ServiceResult<string>.Failure("Şifre sıfırlanırken bir hata oluştu.", HttpStatusCode.InternalServerError);
        }
        catch (InvalidOperationException ex) when (ex.Message == "ADD_PASSWORD_FAILED")
        {
            logger.LogError("Failed to add new password for user {UserId} during admin password reset", id);
            return ServiceResult<string>.Failure("Yeni şifre atanırken bir hata oluştu.", HttpStatusCode.InternalServerError);
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("EMAIL_FAILED:"))
        {
            logger.LogError("Password created but email failed to send for user {UserId}", id);
            string errorMessage = ex.Message.Replace("EMAIL_FAILED:", "");
            return ServiceResult<string>.Failure($"Şifre oluşturuldu ancak email gönderilemedi: {errorMessage}", HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
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

    /// <summary>
    /// Kullanıcının rollerini değiştirir.
    /// </summary>
    public async Task<ServiceResult> ChangeUserRoleAsync(ChangeUserRoleRequest request)
    {
        AppUser? user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        IList<string> currentRoles = await userManager.GetRolesAsync(user);
        IEnumerable<string> rolesToAdd = request.Roles.Except(currentRoles);
        IEnumerable<string> rolesToRemove = currentRoles.Except(request.Roles);

        if (rolesToAdd.Any())
        {
            IdentityResult addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                List<string> errors = addResult.Errors.Select(e => e.Description).ToList();
                return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
            }
        }

        if (rolesToRemove.Any())
        {
            IdentityResult removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                List<string> errors = removeResult.Errors.Select(e => e.Description).ToList();
                return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
            }
        }

        // Security stamp'i güncelle ki kullanıcının token'ı geçersiz olsun ve yeni rollerle tekrar giriş yapsın/token alsın.
        await userManager.UpdateSecurityStampAsync(user);
        await cacheService.RemoveAsync($"security_stamp:{user.Id}");

        logger.LogInformation("User roles updated for user {UserId}. Added: {Added}, Removed: {Removed}", request.UserId, string.Join(",", rolesToAdd), string.Join(",", rolesToRemove));
        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Refresh token ile kullanıcıyı bulur (sayfa yenileme durumu için).
    /// </summary>
    public async Task<ServiceResult<UserWithRolesResponse>> FindUserByRefreshTokenAsync(string refreshToken)
    {
        // Refresh token'ı hash'le
        string hashedToken = RefreshTokenHasher.HashRefreshToken(refreshToken);

        // Direkt veritabanında hash ile ara (index kullanabilir)
        AppUser? user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == hashedToken && u.RefreshTokenExpires > DateTime.UtcNow);

        if (user == null)
        {
            return ServiceResult<UserWithRolesResponse>.Failure("Refresh token ile kullanıcı bulunamadı", HttpStatusCode.Unauthorized);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        return ServiceResult<UserWithRolesResponse>.Success(new UserWithRolesResponse
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!,
            SecurityStamp = user.SecurityStamp,
            Roles = roles.ToList()
        }, HttpStatusCode.OK);
    }
}
