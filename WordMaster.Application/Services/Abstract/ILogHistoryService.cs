using WordMaster.Application.ViewModels.Admin;
using WordMaster.Application.ViewModels.User;

namespace WordMaster.Application.Services.Abstract;

public interface ILogHistoryService
{
    Task RecordAsync(string? appUserId, string? email, string? ipAddress, bool isSuccessful, string source);
    Task<UserLoginStatsViewModel> GetUserLoginStatsAsync(string userId);
    Task<LastLoginInfoViewModel> GetLastSuccessfulLoginAsync(string userId);
    Task<LoginStatisticsViewModel> GetLoginStatisticsAsync();
}

