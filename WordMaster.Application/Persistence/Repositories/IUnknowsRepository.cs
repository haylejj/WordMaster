using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface IUnknowsRepository : IGenericRepository<Unknows>
{
    Task<Unknows?> GetByIdForUserAsync(int unknowsId, string userId);
    Task<Unknows?> GetByWordForUserAsync(int wordId, string userId);
    Task<Unknows?> GetUnknowsWithWordAsync(int unknowsId, string userId);
    Task<List<Unknows>> GetUserUnknowsWithWordAsync(string userId);
    Task<(List<Unknows> Unknows, int TotalCount)> GetPagedUnknowsAsync(string userId, string? search, int page, int pageSize);
    Task<Unknows?> GetLastUnknows();
}
