using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.Services;

public class LogHistoryService(ILogHistoryRepository logHistoryRepository, IUnitOfWork unitOfWork) : ILogHistoryService
{
    private readonly ILogHistoryRepository _logHistoryRepository = logHistoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task RecordAsync(string? appUserId, string? email, string? ipAddress, bool isSuccessful, string source)
    {
        var log = new LogHistory
        {
            AppUserId = string.IsNullOrWhiteSpace(appUserId) ? null : appUserId,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress,
            IsSuccessful = isSuccessful,
            Source = string.IsNullOrWhiteSpace(source) ? null : source,
            AttemptedAt = DateTime.UtcNow
        };

        await _logHistoryRepository.AddAsync(log);
        await _unitOfWork.CommitAsync();
    }
}

