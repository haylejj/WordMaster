using WordMaster.Application.Dto.Favorite;
using WordMaster.Application.Requests.Favorite;
using WordMaster.Application.Requests.Word;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFavoriteService
{
    Task<List<FavoriteWithWordDto>> GetUserFavoritesAsync(Guid userId);
    Task<ServiceResult<bool>> ToggleFavoriteAsync(ToggleFavoriteRequest request, Guid userId);
    Task<ServiceResult<FavoriteWithWordDto>> GetFavoriteWithWordAsync(int favoriteId, Guid userId);
    Task<ServiceResult> DeleteFavoriteAsync(int favoriteId, Guid userId);
    Task<ServiceResult<string>> GetRandomWordFromFavoritesAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request);
    Task<ServiceResult<PagedResult<FavoriteWithWordDto>>> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize);
}
