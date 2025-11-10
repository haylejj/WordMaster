using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.Services;

public class AdminDashboardService(AppDbContext context) : IAdminDashboardService
{
    private readonly AppDbContext _context = context;

    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var totalWordsTask = await _context.Words.CountAsync();
        var totalFavoritesTask = await _context.Favorites.CountAsync();
        var totalUnknowsTask = await _context.Unknows.CountAsync();
        var totalUsersTask = await _context.Users.CountAsync();
        var totalLoginsTask = await _context.LogHistories.CountAsync();
        var successfulLoginsTask = await _context.LogHistories.CountAsync(x => x.IsSuccessful);
        var failedLoginsTask = await _context.LogHistories.CountAsync(x => !x.IsSuccessful);

        var startDate = DateTime.UtcNow.Date.AddDays(-6);

        var dailyStatsQuery = await _context.LogHistories
            .Where(x => x.AttemptedAt >= startDate)
            .GroupBy(x => x.AttemptedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Success = g.Count(x => x.IsSuccessful),
                Fail = g.Count(x => !x.IsSuccessful)
            })
            .ToListAsync();

        var dailyLookup = dailyStatsQuery.ToDictionary(
            k => DateOnly.FromDateTime(k.Date),
            v => new DailyLoginStatViewModel
            {
                Date = DateOnly.FromDateTime(v.Date),
                SuccessCount = v.Success,
                FailCount = v.Fail
            });

        var dailyStats = new List<DailyLoginStatViewModel>(7);
        for (int i = 0; i < 7; i++)
        {
            var date = DateOnly.FromDateTime(startDate.AddDays(i));
            if (!dailyLookup.TryGetValue(date, out var value))
            {
                value = new DailyLoginStatViewModel
                {
                    Date = date,
                    SuccessCount = 0,
                    FailCount = 0
                };
            }
            dailyStats.Add(value);
        }

        return new AdminDashboardViewModel
        {
            TotalWords = totalWordsTask,
            TotalFavorites = totalFavoritesTask,
            TotalUnknows = totalUnknowsTask,
            TotalUsers = totalUsersTask,
            TotalLogins = totalLoginsTask,
            SuccessfulLogins = successfulLoginsTask,
            FailedLogins = failedLoginsTask,
            DailyLoginStats = dailyStats
        };
    }
}

