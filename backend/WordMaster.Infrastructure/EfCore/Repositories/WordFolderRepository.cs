using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class WordFolderRepository(AppDbContext context) : GenericRepository<WordFolder>(context), IWordFolderRepository
{
    public async Task<List<Word>> GetWordsInFolderAsync(long folderId)
    {
        return await _context.WordFolders
            .AsNoTracking()
            .Where(wf => wf.FolderId == folderId)
            .Include(wf => wf.Word)
            .Select(wf => wf.Word)
            .ToListAsync();
    }

    public async Task<bool> LinkExistsAsync(long folderId, long wordId)
    {
        return await _context.WordFolders.AnyAsync(x => x.FolderId == folderId && x.WordId == wordId);
    }

    public async Task<WordFolder?> GetLinkAsync(long folderId, long wordId)
    {
        return await _context.WordFolders.FirstOrDefaultAsync(x => x.FolderId == folderId && x.WordId == wordId);
    }
}


