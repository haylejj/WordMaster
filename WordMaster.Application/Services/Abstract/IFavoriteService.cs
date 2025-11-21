using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFavoriteService
{
    Task<List<Favorite>> GetUserFavoritesAsync(Guid userId);
    Task<Result<bool>> ToggleFavoriteAsync(long wordId, Guid userId);
    Task<Result<Favorite>> GetFavoriteWithWordAsync(int favoriteId, Guid userId);
    Task<Result> DeleteFavoriteAsync(int favoriteId, Guid userId);
    Task<Result<string>> GetRandomWordFromFavoritesAsync(Guid userId);
    Task<Result<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<Result<(List<Favorite> Favorites, int TotalCount)>> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize);
}
