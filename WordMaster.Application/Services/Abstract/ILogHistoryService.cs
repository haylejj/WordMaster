namespace WordMaster.Application.Services.Abstract;

public interface ILogHistoryService
{
    Task RecordAsync(string? appUserId, string? email, string? ipAddress, bool isSuccessful, string source);
}

