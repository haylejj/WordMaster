using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Responses.Admin;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class AdminDashboardService(
    IWordRepository wordRepository,
    IFavoriteRepository favoriteRepository,
    IUnknowsRepository unknowsRepository,
    IFolderRepository folderRepository,
    IWordFolderRepository wordFolderRepository,
    ILogHistoryRepository logHistoryRepository,
    ILogHistoryService logHistoryService,
    UserManager<AppUser> userManager) : IAdminDashboardService
{
    public async Task<ServiceResult<AdminDashboardResponse>> GetDashboardAsync()
    {
        DateTime yesterday = DateTime.UtcNow.AddHours(-24);

        int totalWordsTask = await wordRepository.CountAsync();
        int totalFavoritesTask = await favoriteRepository.CountAsync();
        int totalUnknowsTask = await unknowsRepository.CountAsync();
        int totalFoldersTask = await folderRepository.CountAsync();
        int totalUsersTask = await userManager.Users.CountAsync();
        int lockedUsersTask = await userManager.Users.CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);

        // 24h Metrics
        int newWords24h = await wordRepository.CountAsync(w => w.CreatedTime > yesterday);
        int newFolders24h = await folderRepository.CountAsync(f => f.CreatedTime > yesterday);

        // Active Users (Unique successful logins in last 24h)
        int activeUsers24h = await logHistoryRepository
            .Where(l => l.AttemptedAt > yesterday && l.IsSuccessful && l.AppUserId != null)
            .Select(l => l.AppUserId)
            .Distinct()
            .CountAsync();

        // Entity Stats - Last IDs
        // Using GetAll() which returns IQueryable, then OrderByDescending + Select + FirstOrDefault
        long lastWordId = await wordRepository.GetAll().OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
        long lastUnknowsId = await unknowsRepository.GetAll().OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
        long lastFavoriteId = await favoriteRepository.GetAll().OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();

        // Total Words in Folders
        int totalWordsInFolders = await wordFolderRepository.CountAsync();

        LoginStatisticsResponse loginStatistics = await logHistoryService.GetLoginStatisticsAsync();

        AdminDashboardResponse dashboard = new()
        {
            TotalFolders = totalFoldersTask,
            TotalWords = totalWordsTask,
            TotalFavorites = totalFavoritesTask,
            TotalUnknows = totalUnknowsTask,
            TotalUsers = totalUsersTask,
            LockedUserCount = lockedUsersTask,
            ActiveUsers24h = activeUsers24h,
            NewWords24h = newWords24h,
            NewFolders24h = newFolders24h,
            LastWordId = lastWordId,
            LastUnknowsId = lastUnknowsId,
            LastFavoriteId = lastFavoriteId,
            TotalWordsInFolders = totalWordsInFolders,
            TotalLogins = loginStatistics.TotalLogins,
            SuccessfulLogins = loginStatistics.SuccessfulLogins,
            FailedLogins = loginStatistics.FailedLogins,
            DailyLoginStats = loginStatistics.DailyLoginStats
        };

        return ServiceResult<AdminDashboardResponse>.Success(dashboard, HttpStatusCode.OK);
    }
}
