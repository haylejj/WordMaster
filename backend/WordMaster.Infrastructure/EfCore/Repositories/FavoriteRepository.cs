using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class FavoriteRepository(AppDbContext context) : GenericRepository<Favorite>(context), IFavoriteRepository
{
    public async Task<Favorite?> GetByWordForUserAsync(long wordId, Guid userId)
    {
        return await _context.Favorites
            .FirstOrDefaultAsync(x => x.WordId == wordId && x.UserId == userId);
    }
    public async Task<List<Favorite>> GetUserFavoritesWithWordAsync(Guid userId)
    {
        return await _context.Favorites
            .Include(x => x.Word)
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<(List<Favorite> Favorites, int TotalCount)> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize)
    {
        IQueryable<Favorite> query = _context.Favorites
            .Include(x => x.Word)
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Word!.EnglishWord!.Contains(search) || x.Word!.TurkishWord!.Contains(search));
        }

        int totalCount = await query.CountAsync();
        List<Favorite> favorites = await query
            .OrderBy(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (favorites, totalCount);
    }
}
