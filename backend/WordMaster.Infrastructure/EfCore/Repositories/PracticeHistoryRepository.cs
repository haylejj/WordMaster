using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class PracticeHistoryRepository(AppDbContext context) : GenericRepository<PracticeHistory>(context), IPracticeHistoryRepository
{
    public async Task<List<PracticeHistory>> GetHistoryByUserAsync(Guid userId, DateTime startDate)
    {
        return await _context.PracticeHistories
            .Where(x => x.UserId == userId && x.PracticeDate >= startDate)
            .OrderBy(x => x.PracticeDate)
            .ToListAsync();
    }
}
