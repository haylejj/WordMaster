using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class UnknowsRepository(AppDbContext context) : GenericRepository<Unknows>(context), IUnknowsRepository
{
    public async Task<Unknows?> GetByIdForUserAsync(int unknowsId, string userId)
    {
        return await _context.Unknows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == unknowsId && x.UserId == userId);
    }

    public async Task<Unknows?> GetByWordForUserAsync(int wordId, string userId)
    {
        return await _context.Unknows
            .FirstOrDefaultAsync(x => x.WordId == wordId && x.UserId == userId);
    }

    public async Task<Unknows?> GetUnknowsWithWordAsync(int unknowsId, string userId)
    {
        return await _context.Unknows
            .Include(x => x.Word)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == unknowsId && x.UserId == userId);
    }

    public async Task<List<Unknows>> GetUserUnknowsWithWordAsync(string userId)
    {
        return await _context.Unknows
            .Include(x => x.Word)
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<Unknows> Unknows, int TotalCount)> GetPagedUnknowsAsync(string userId, string? search, int page, int pageSize)
    {
        IQueryable<Unknows> query = _context.Unknows
            .Include(x => x.Word)
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Word!.EnglishWord!.Contains(search) || x.Word!.TurkishWord!.Contains(search));
        }

        int totalCount = await query.CountAsync();
        List<Unknows> items = await query
            .OrderBy(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Unknows?> GetLastUnknows()
    {
        return await _context.Unknows
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }
}
