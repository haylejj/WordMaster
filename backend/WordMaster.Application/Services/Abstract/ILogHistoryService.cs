using WordMaster.Application.Requests.LogHistory;
using WordMaster.Application.Responses.Admin;
using WordMaster.Application.Responses.LogHistory;
using WordMaster.Application.Responses.User;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface ILogHistoryService
{
    Task RecordAsync(string? appUserId, string? email, string? ipAddress, bool isSuccessful, string source);
    Task<UserLoginStatsResponse> GetUserLoginStatsAsync(string userId);
    Task<LastLoginInfoResponse> GetLastSuccessfulLoginAsync(string userId);
    Task<LoginStatisticsResponse> GetLoginStatisticsAsync();
    Task<ServiceResult<PagedResult<LogHistoryResponse>>> GetPagedLogHistoryAsync(GetLogHistoryRequest request);
}

