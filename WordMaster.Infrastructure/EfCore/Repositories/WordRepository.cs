using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class WordRepository(AppDbContext context) : GenericRepository<Word>(context), IWordRepository
{
    public async Task<Word?> GetWordForUserAsync(int wordId, string userId)
    {
        return await _context.Words
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == wordId && x.UserId == userId);
    }

    public async Task<Word?> GetWordForUserTrackedAsync(int wordId, string userId)
    {
        return await _context.Words
            .FirstOrDefaultAsync(x => x.Id == wordId && x.UserId == userId);
    }

    public async Task<Word?> GetWordByNormalizedEnglishAsync(string userId, string normalizedEnglishWord)
    {
        return await _context.Words
            .FirstOrDefaultAsync(x => x.UserId == userId && x.EnglishWord != null && x.EnglishWord == normalizedEnglishWord);
    }

    public async Task<List<Word>> GetWordsByUserAsync(string userId)
    {
        return await _context.Words
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<Word> Words, int TotalCount)> GetPagedWordsAsync(string userId, string? search, int page, int pageSize)
    {
        IQueryable<Word> query = _context.Words
            .Include(x => x.Favorite)
            .Include(x => x.Unknows)
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.EnglishWord!.Contains(search) || x.TurkishWord!.Contains(search));
        }

        int totalCount = await query.CountAsync();
        List<Word> words = await query
            .OrderBy(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (words, totalCount);
    }

    public async Task<Word?> GetLastWord()
    {
        return await _context.Words
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }
}
