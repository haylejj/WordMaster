using Core.Entity;

namespace Core.Repositories;

public interface IFavoriteRepository : IGenericRepository<Favorite>
{
    Task<Favorite> GetLastFavorite();
}
