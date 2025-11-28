using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface IFavoriteRepository : IGenericRepository<Favorite>
{
    Task<Favorite?> GetByIdForUserAsync(int favoriteId, Guid userId);
    Task<Favorite?> GetByWordForUserAsync(long wordId, Guid userId);
    Task<Favorite?> GetFavoriteWithWordAsync(int favoriteId, Guid userId);
    Task<List<Favorite>> GetUserFavoritesWithWordAsync(Guid userId);
    Task<(List<Favorite> Favorites, int TotalCount)> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize);
    Task<Favorite?> GetLastFavorite();
}