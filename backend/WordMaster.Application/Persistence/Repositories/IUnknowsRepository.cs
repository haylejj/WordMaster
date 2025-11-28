using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface IUnknowsRepository : IGenericRepository<Unknows>
{
    Task<Unknows?> GetByIdForUserAsync(int unknowsId, Guid userId);
    Task<Unknows?> GetByWordForUserAsync(long wordId, Guid userId);
    Task<Unknows?> GetUnknowsWithWordAsync(int unknowsId, Guid userId);
    Task<List<Unknows>> GetUserUnknowsWithWordAsync(Guid userId);
    Task<(List<Unknows> Unknows, int TotalCount)> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize);
    Task<Unknows?> GetLastUnknows();
}
