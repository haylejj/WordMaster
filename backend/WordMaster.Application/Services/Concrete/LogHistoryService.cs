using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.LogHistory;
using WordMaster.Application.Responses.Admin;
using WordMaster.Application.Responses.LogHistory;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class LogHistoryService(ILogHistoryRepository logHistoryRepository, IUnitOfWork unitOfWork, ILogger<LogHistoryService> logger) : ILogHistoryService
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

        if (lastLogin == null)
        {
            return new LastLoginInfoResponse
            {
                LastLoginDate = null,
                LastLoginIpAddress = null
            };
        }

        return new LastLoginInfoResponse
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
    public async Task<ServiceResult<PagedResult<LogHistoryResponse>>> GetPagedLogHistoryAsync(GetLogHistoryRequest request)
    {
        if (request.Page < 1) request.Page = 1;
        if (request.Size <= 0) request.Size = 20;

        IQueryable<LogHistory> query = logHistoryRepository.GetAll().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            string term = request.SearchTerm.Trim();
            query = query.Where(x =>
                (request.SearchInUserId && x.AppUserId.ToString().Contains(term)) ||
                (request.SearchInEmail && x.Email.Contains(term)) ||
                (request.SearchInIp && x.IpAddress.Contains(term))
            );
        }

        int totalCount = await query.CountAsync();

        List<LogHistory> logs = await query
            .OrderByDescending(x => x.AttemptedAt)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync();

        List<LogHistoryResponse> responseItems = logs.Select(x => new LogHistoryResponse
        {
            Id = x.Id,
            AppUserId = x.AppUserId,
            Email = x.Email,
            IpAddress = x.IpAddress,
            IsSuccessful = x.IsSuccessful,
            AttemptedAt = x.AttemptedAt,
            Source = x.Source
        }).ToList();

        PagedResult<LogHistoryResponse> result = new()
        {
            Items = responseItems,
            PageNumber = request.Page,
            PageSize = request.Size,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<LogHistoryResponse>>.Success(result, HttpStatusCode.OK);
    }
}

