using Core.Entity;
using System.Linq.Expressions;
using Core.Results;

namespace Core.Service;

public interface IFavoriteService
{
    IQueryable<Favorite> Where(Expression<Func<Favorite, bool>> predicate);
    Task<List<Favorite>> GetUserFavoritesAsync(string userId);
    Task<Result<bool>> ToggleFavoriteAsync(int wordId, string userId);
    Task<Result<Favorite>> GetFavoriteWithWordAsync(int favoriteId, string userId);
    Task<Result> DeleteFavoriteAsync(int favoriteId, string userId);
    Task<Result<string>> GetRandomWordFromFavoritesAsync(string userId);
    Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord);
    Task<Result<(List<Favorite> Favorites, int TotalCount)>> GetPagedFavoritesAsync(string userId, string? search, int page, int pageSize);
}
