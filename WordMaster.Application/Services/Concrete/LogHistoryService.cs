using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Responses.Admin;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
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

    public async Task<UserLoginStatsResponse> GetUserLoginStatsAsync(string userId)
    {
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return new UserLoginStatsResponse();
        }

        int totalLogins = await logHistoryRepository.CountAsync(x => x.AppUserId == userGuid);

        int successfulLogins = await logHistoryRepository.CountAsync(x => x.AppUserId == userGuid && x.IsSuccessful);
        int failedLogins = totalLogins - successfulLogins;

        return new UserLoginStatsResponse
        {
            TotalLogins = totalLogins,
            SuccessfulLogins = successfulLogins,
            FailedLogins = failedLogins
        };
    }

    public async Task<LastLoginInfoResponse> GetLastSuccessfulLoginAsync(string userId)
    {
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return new LastLoginInfoResponse
            {
                LastLoginDate = null,
                LastLoginIpAddress = null
            };
        }

        LogHistory? lastLogin = await logHistoryRepository.GetLastSuccessfulLoginAsync(userGuid);

        return lastLogin == null
            ? new LastLoginInfoResponse
            {
                LastLoginDate = null,
                LastLoginIpAddress = null
            }
            : new LastLoginInfoResponse
            {
                LastLoginDate = lastLogin.AttemptedAt,
                LastLoginIpAddress = lastLogin.IpAddress
            };
    }

    public async Task<LoginStatisticsResponse> GetLoginStatisticsAsync()
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

        Dictionary<DateOnly, DailyLoginStatResponse> dailyLookup = dailyStatsQuery.ToDictionary(
            k => DateOnly.FromDateTime(k.Date),
            v => new DailyLoginStatResponse
            {
                Date = DateOnly.FromDateTime(v.Date),
                SuccessCount = v.Success,
                FailCount = v.Fail
            });

        List<DailyLoginStatResponse> dailyStats = new(7);
        for (int i = 0; i < 7; i++)
        {
            DateOnly date = DateOnly.FromDateTime(startDate.AddDays(i));
            if (!dailyLookup.TryGetValue(date, out DailyLoginStatResponse? value))
            {
                value = new DailyLoginStatResponse
                {
                    Date = date,
                    SuccessCount = 0,
                    FailCount = 0
                };
            }
            dailyStats.Add(value);
        }

        return new LoginStatisticsResponse
        {
            TotalLogins = totalLogins,
            SuccessfulLogins = successfulLogins,
            FailedLogins = failedLogins,
            DailyLoginStats = dailyStats
        };
    }
}

