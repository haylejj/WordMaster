using WordMaster.Application.Responses.Admin;
using WordMaster.Application.Responses.User;

namespace WordMaster.Application.Services.Abstract;

public interface ILogHistoryService
{
    Task RecordAsync(string? appUserId, string? email, string? ipAddress, bool isSuccessful, string source);
    Task<UserLoginStatsResponse> GetUserLoginStatsAsync(string userId);
    Task<LastLoginInfoResponse> GetLastSuccessfulLoginAsync(string userId);
    Task<LoginStatisticsResponse> GetLoginStatisticsAsync();
}

