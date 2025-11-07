using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface IFavoriteRepository : IGenericRepository<Favorite>
{
    Task<Favorite?> GetByIdForUserAsync(int favoriteId, string userId);
    Task<Favorite?> GetByWordForUserAsync(int wordId, string userId);
    Task<Favorite?> GetFavoriteWithWordAsync(int favoriteId, string userId);
    Task<List<Favorite>> GetUserFavoritesWithWordAsync(string userId);
    Task<(List<Favorite> Favorites, int TotalCount)> GetPagedFavoritesAsync(string userId, string? search, int page, int pageSize);
    Task<Favorite?> GetLastFavorite();
}