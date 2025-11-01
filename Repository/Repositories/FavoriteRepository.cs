using Core.Entity;
using Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories;

public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
{
    public FavoriteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Favorite> GetLastFavorite()
    {
        return await _context.Favorites.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    }
}
