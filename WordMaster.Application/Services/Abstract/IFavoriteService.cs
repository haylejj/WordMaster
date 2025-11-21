using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFavoriteService
{
    Task<List<Favorite>> GetUserFavoritesAsync(Guid userId);
    Task<ServiceResult<bool>> ToggleFavoriteAsync(long wordId, Guid userId);
    Task<ServiceResult<Favorite>> GetFavoriteWithWordAsync(int favoriteId, Guid userId);
    Task<ServiceResult> DeleteFavoriteAsync(int favoriteId, Guid userId);
    Task<ServiceResult<string>> GetRandomWordFromFavoritesAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<ServiceResult<(List<Favorite> Favorites, int TotalCount)>> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize);
}
