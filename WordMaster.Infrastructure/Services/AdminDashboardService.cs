using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Admin;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class AdminDashboardService(IWordRepository wordRepository, IFavoriteRepository favoriteRepository, IUnknowsRepository unknowsRepository, ILogHistoryService logHistoryService, UserManager<AppUser> userManager) : IAdminDashboardService
{
    public async Task<ServiceResult<AdminDashboardViewModel>> GetDashboardAsync()
    {
        int totalWordsTask = await wordRepository.CountAsync();
        int totalFavoritesTask = await favoriteRepository.CountAsync();
        int totalUnknowsTask = await unknowsRepository.CountAsync();
        int totalUsersTask = await userManager.Users.CountAsync();

        LoginStatisticsViewModel loginStatistics = await logHistoryService.GetLoginStatisticsAsync();

        var dashboard = new AdminDashboardViewModel
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

        return ServiceResult<AdminDashboardViewModel>.Success(dashboard, HttpStatusCode.OK);
    }
}
