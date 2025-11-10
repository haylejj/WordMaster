using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface ILogHistoryRepository : IGenericRepository<LogHistory>
{
    Task<LogHistory?> GetLastSuccessfulLoginAsync(string userId);
}

