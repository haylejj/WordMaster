using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Net;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.Services;

public class DatabaseService(
    AppDbContext context,
    UserManager<AppUser> userManager,
    ILogHistoryService logHistoryService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<DatabaseService> logger) : IDatabaseService
{
    public async Task<ServiceResult> ResetTableAsync(string tableName, string password, string userId, string? targetUserId = null)
    {
        // Pre-validation outside transaction
        AppUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Database reset failed: User '{UserId}' not found.", userId);
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        bool checkPassword = await userManager.CheckPasswordAsync(user, password);
        if (!checkPassword)
        {
            logger.LogWarning("Database reset failed: Invalid password for user '{UserId}'.", userId);
            return ServiceResult.Failure("Geçersiz şifre.", HttpStatusCode.Unauthorized);
        }

        bool isTargetUserIdProvided = !string.IsNullOrEmpty(targetUserId);
        if (isTargetUserIdProvided)
        {
            AppUser? targetUser = await userManager.FindByIdAsync(targetUserId!);
            if (targetUser == null)
            {
                logger.LogWarning("Database reset failed: Target User '{TargetUserId}' not found.", targetUserId);
                return ServiceResult.Failure("Hedef kullanıcı bulunamadı.", HttpStatusCode.NotFound);
            }
        }

        // Validate table name before transaction
        string tableNameLower = tableName.ToLower();
        if (tableNameLower != "words" && tableNameLower != "unknows" && tableNameLower != "favorites" && tableNameLower != "folders" && tableNameLower != "folderwords")
        {
            logger.LogWarning("Database reset failed: Invalid table name '{TableName}'.", tableName);
            return ServiceResult.Failure("Geçersiz tablo adı.", HttpStatusCode.BadRequest);
        }

        // Use execution strategy to handle SqlServerRetryingExecutionStrategy with transactions
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

                switch (tableNameLower)
                {
                    case "words":
                        if (!isTargetUserIdProvided)
                        {
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM WordFolder");
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Favorites");
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Unknows");
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Words");
                        }
                        else
                        {
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM WordFolder WHERE WordId IN (SELECT Id FROM Words WHERE UserId = {0})", targetUserId!);
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Favorites WHERE UserId = {0}", targetUserId!);
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Unknows WHERE UserId = {0}", targetUserId!);
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Words WHERE UserId = {0}", targetUserId!);
                        }
                        break;

                    case "unknows":
                        if (!isTargetUserIdProvided)
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Unknows");
                        else
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Unknows WHERE UserId = {0}", targetUserId!);
                        break;

                    case "favorites":
                        if (!isTargetUserIdProvided)
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Favorites");
                        else
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Favorites WHERE UserId = {0}", targetUserId!);
                        break;

                    case "folders":
                        if (!isTargetUserIdProvided)
                        {
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM WordFolder");
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Folders");
                        }
                        else
                        {
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM WordFolder WHERE FolderId IN (SELECT Id FROM Folders WHERE UserId = {0})", targetUserId!);
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM Folders WHERE UserId = {0}", targetUserId!);
                        }
                        break;

                    case "folderwords":
                        if (!isTargetUserIdProvided)
                        {
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM WordFolder");
                        }
                        else
                        {
                            await context.Database.ExecuteSqlRawAsync("DELETE FROM WordFolder WHERE FolderId IN (SELECT Id FROM Folders WHERE UserId = {0})", targetUserId!);
                        }
                        break;
                }

                string? ipAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string actionDetails = !isTargetUserIdProvided
                    ? $"Admin Database Reset: {tableName} (ALL)"
                    : $"Admin Database Reset: {tableName} (TargetUser: {targetUserId})";

                await logHistoryService.RecordAsync(
                    userId,
                    user.Email,
                    ipAddress,
                    true,
                    actionDetails
                );

                await transaction.CommitAsync();
            });

            logger.LogInformation("Database reset successful for table '{TableName}' by user '{UserId}'. Target: {Target}", tableName, userId, targetUserId ?? "ALL");
            return ServiceResult.Success(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database reset error for table '{TableName}' by user '{UserId}'.", tableName, userId);
            return ServiceResult.Failure($"Veritabanı sıfırlama hatası: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
