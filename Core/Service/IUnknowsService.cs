using Core.Entity;
using System.Linq.Expressions;

namespace Core.Service;

public interface IUnknowsService
{
    IQueryable<Unknows> Where(Expression<Func<Unknows, bool>> predicate);
    Task AddAsync(Unknows entity);
    Task RemoveAsync(Unknows entity);
    Task<(bool Success, string? ErrorMessage)> DeleteUnknowsAsync(int unknowsId, string userId);
    Task<string> GetRandomWordFromUnknowsAsync(string userId);
    Task<bool> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord);
}
