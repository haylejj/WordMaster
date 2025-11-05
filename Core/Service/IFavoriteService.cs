using Core.Entity;
using System.Linq.Expressions;

namespace Core.Service;

public interface IFavoriteService
{
    IQueryable<Favorite> Where(Expression<Func<Favorite, bool>> predicate);
    Task AddAsync(Favorite entity);
    Task RemoveAsync(Favorite entity);
    Task<(bool Success, string? ErrorMessage)> DeleteFavoriteAsync(int favoriteId, string userId);
    Task<string> GetRandomWordFromFavoritesAsync(string userId);
    Task<bool> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord, IWordService wordService);
}
