using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface IPracticeHistoryRepository : IGenericRepository<PracticeHistory>
{
    Task<List<PracticeHistory>> GetHistoryByUserAsync(Guid userId, DateTime startDate);
}
