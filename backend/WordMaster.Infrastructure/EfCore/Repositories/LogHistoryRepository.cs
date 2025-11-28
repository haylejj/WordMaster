using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class LogHistoryRepository(AppDbContext context) : GenericRepository<LogHistory>(context), ILogHistoryRepository
{
    public async Task<LogHistory?> GetLastSuccessfulLoginAsync(Guid userId)
    {
        return await _context.LogHistories
            .Where(x => x.AppUserId == userId && x.IsSuccessful)
            .OrderByDescending(x => x.AttemptedAt)
            .FirstOrDefaultAsync();
    }
}

