using Core.Entity;
using System.Linq.Expressions;
using Core.Results;

namespace Core.Service;

public interface IUnknowsService
{
    IQueryable<Unknows> Where(Expression<Func<Unknows, bool>> predicate);
    Task<List<Unknows>> GetUserUnknowsAsync(string userId);
    Task<Result<bool>> ToggleUnknowsAsync(int wordId, string userId);
    Task<Result<Unknows>> GetUnknowsWithWordAsync(int unknowsId, string userId);
    Task<Result> DeleteUnknowsAsync(int unknowsId, string userId);
    Task<Result<string>> GetRandomWordFromUnknowsAsync(string userId);
    Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord);
    Task<Result<(List<Unknows> Unknows, int TotalCount)>> GetPagedUnknowsAsync(string userId, string? search, int page, int pageSize);
}
