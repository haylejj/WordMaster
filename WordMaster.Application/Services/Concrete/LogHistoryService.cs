using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Admin;
using WordMaster.Application.ViewModels.User;
using WordMaster.Domain.Entities;

namespace WordMaster.Application.Services.Concrete;

public class LogHistoryService(ILogHistoryRepository logHistoryRepository, IUnitOfWork unitOfWork) : ILogHistoryService
{
    public async Task RecordAsync(string? appUserId, string? email, string? ipAddress, bool isSuccessful, string source)
    {
        Guid? parsedUserId = null;
        if (!string.IsNullOrWhiteSpace(appUserId) && Guid.TryParse(appUserId, out Guid userId))
        {
            parsedUserId = userId;
        }

        LogHistory log = new()
        {
            AppUserId = parsedUserId,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress,
            IsSuccessful = isSuccessful,
            Source = string.IsNullOrWhiteSpace(source) ? null : source,
            AttemptedAt = DateTime.UtcNow
        };

        await logHistoryRepository.AddAsync(log);
        await unitOfWork.CommitAsync();
    }

    public async Task<UserLoginStatsViewModel> GetUserLoginStatsAsync(string userId)
    {
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return new UserLoginStatsViewModel();
        }

        int totalLogins = await logHistoryRepository.CountAsync(x => x.AppUserId == userGuid);

        int successfulLogins = await logHistoryRepository.CountAsync(x => x.AppUserId == userGuid && x.IsSuccessful);
        int failedLogins = totalLogins - successfulLogins;

        return new UserLoginStatsViewModel
        {
            TotalLogins = totalLogins,
            SuccessfulLogins = successfulLogins,
            FailedLogins = failedLogins
        };
    }

    public async Task<LastLoginInfoViewModel> GetLastSuccessfulLoginAsync(string userId)
    {
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return new LastLoginInfoViewModel
            {
                LastLoginDate = null,
                LastLoginIpAddress = null
            };
        }

        LogHistory? lastLogin = await logHistoryRepository.GetLastSuccessfulLoginAsync(userGuid);

        return lastLogin == null
            ? new LastLoginInfoViewModel
            {
                LastLoginDate = null,
                LastLoginIpAddress = null
            }
            : new LastLoginInfoViewModel
            {
                LastLoginDate = lastLogin.AttemptedAt,
                LastLoginIpAddress = lastLogin.IpAddress
            };
    }

    public async Task<LoginStatisticsViewModel> GetLoginStatisticsAsync()
    {
        int totalLogins = await logHistoryRepository.CountAsync();
        int successfulLogins = await logHistoryRepository.CountAsync(x => x.IsSuccessful);
        int failedLogins = await logHistoryRepository.CountAsync(x => !x.IsSuccessful);

        DateTime startDate = DateTime.UtcNow.Date.AddDays(-6);

        var dailyStatsQuery = await logHistoryRepository
            .Where(x => x.AttemptedAt >= startDate)
            .GroupBy(x => x.AttemptedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Success = g.Count(x => x.IsSuccessful),
                Fail = g.Count(x => !x.IsSuccessful)
            })
            .ToListAsync();

        Dictionary<DateOnly, DailyLoginStatViewModel> dailyLookup = dailyStatsQuery.ToDictionary(
            k => DateOnly.FromDateTime(k.Date),
            v => new DailyLoginStatViewModel
            {
                Date = DateOnly.FromDateTime(v.Date),
                SuccessCount = v.Success,
                FailCount = v.Fail
            });

        List<DailyLoginStatViewModel> dailyStats = new(7);
        for (int i = 0; i < 7; i++)
        {
            DateOnly date = DateOnly.FromDateTime(startDate.AddDays(i));
            if (!dailyLookup.TryGetValue(date, out DailyLoginStatViewModel? value))
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

        return new LoginStatisticsViewModel
        {
            TotalLogins = totalLogins,
            SuccessfulLogins = successfulLogins,
            FailedLogins = failedLogins,
            DailyLoginStats = dailyStats
        };
    }
}

