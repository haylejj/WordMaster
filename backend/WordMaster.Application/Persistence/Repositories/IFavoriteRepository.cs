using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface IFavoriteRepository : IGenericRepository<Favorite>
{
    Task<Favorite?> GetByWordForUserAsync(long wordId, Guid userId);
    Task<List<Favorite>> GetUserFavoritesWithWordAsync(Guid userId);
    Task<(List<Favorite> Favorites, int TotalCount)> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize);
}