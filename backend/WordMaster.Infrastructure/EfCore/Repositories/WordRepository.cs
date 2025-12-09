using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class WordRepository(AppDbContext context) : GenericRepository<Word>(context), IWordRepository
{
    public async Task<Word?> GetWordForUserAsync(long wordId, Guid userId)
    {
        return await _context.Words
            .Include(x => x.Favorite)
            .Include(x => x.Unknows)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == wordId && x.UserId == userId);
    }

    public async Task<Word?> GetWordForUserTrackedAsync(long wordId, Guid userId)
    {
        return await _context.Words
            .FirstOrDefaultAsync(x => x.Id == wordId && x.UserId == userId);
    }

    public async Task<Word?> GetWordForUserTrackedWithFoldersAsync(long wordId, Guid userId)
    {
        return await _context.Words
            .Include(x => x.WordFolders)
            .FirstOrDefaultAsync(x => x.Id == wordId && x.UserId == userId);
    }

    public async Task<Word?> GetWordForDeleteAsync(long wordId, Guid userId)
    {
        return await _context.Words
            .Include(x => x.WordFolders)
            .Include(x => x.Favorite)
            .Include(x => x.Unknows)
            .FirstOrDefaultAsync(x => x.Id == wordId && x.UserId == userId);
    }
    public async Task<Word?> GetWordByNormalizedEnglishAsync(Guid userId, string normalizedEnglishWord)
    {
        return await _context.Words
            .FirstOrDefaultAsync(x => x.UserId == userId && x.EnglishWord != null && x.EnglishWord == normalizedEnglishWord);
    }

    public async Task<List<Word>> GetWordsByUserAsync(Guid userId)
    {
        return await _context.Words
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<Word> Words, int TotalCount)> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize)
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
            .OrderByDescending(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (words, totalCount);
    }
    public async Task<(List<Word> Words, int TotalCount)> GetAdminPagedWordsAsync(string? search, int page, int pageSize)
    {
        IQueryable<Word> query = _context.Words
            .Include(x => x.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Try to parse search as ID
            if (long.TryParse(search, out long searchId))
            {
                query = query.Where(x => x.Id == searchId);
            }
            else
            {
                query = query.Where(x =>
                    x.EnglishWord!.Contains(search) ||
                    x.TurkishWord!.Contains(search) ||
                    (x.User != null && x.User.UserName!.Contains(search)));
            }
        }

        int totalCount = await query.CountAsync();
        List<Word> words = await query
            .OrderByDescending(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (words, totalCount);
    }


    public void DeleteWordWithRelations(Word word)
    {
        if (word.WordFolders != null && word.WordFolders.Count != 0)
        {
            _context.WordFolders.RemoveRange(word.WordFolders);
        }
        if (word.Favorite != null)
        {
            _context.Favorites.Remove(word.Favorite);
        }
        if (word.Unknows != null)
        {
            _context.Unknows.Remove(word.Unknows);
        }
        _context.Words.Remove(word);
    }
}
