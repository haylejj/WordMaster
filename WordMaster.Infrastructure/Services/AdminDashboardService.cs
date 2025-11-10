using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.Services;

public class AdminDashboardService(IWordRepository wordRepository, IFavoriteRepository favoriteRepository, IUnknowsRepository unknowsRepository, ILogHistoryService logHistoryService, UserManager<AppUser> userManager) : IAdminDashboardService
{
    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var totalWordsTask = await wordRepository.CountAsync();
        var totalFavoritesTask = await favoriteRepository.CountAsync();
        var totalUnknowsTask = await unknowsRepository.CountAsync();
        var totalUsersTask = await userManager.Users.CountAsync();

        var loginStatistics = await logHistoryService.GetLoginStatisticsAsync();

        return new AdminDashboardViewModel
        {
            TotalWords = totalWordsTask,
            TotalFavorites = totalFavoritesTask,
            TotalUnknows = totalUnknowsTask,
            TotalUsers = totalUsersTask,
            TotalLogins = loginStatistics.TotalLogins,
            SuccessfulLogins = loginStatistics.SuccessfulLogins,
            FailedLogins = loginStatistics.FailedLogins,
            DailyLoginStats = loginStatistics.DailyLoginStats
        };
    }
}

