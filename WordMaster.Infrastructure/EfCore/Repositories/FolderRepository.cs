using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class FolderRepository(AppDbContext context) : GenericRepository<Folder>(context), IFolderRepository
{
    public async Task<List<Folder>> GetUserFoldersAsync(Guid userId)
    {
        return await _context.Folders
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedTime)
            .Include(f => f.WordFolders)
            .ToListAsync();
    }

    public async Task<Folder?> GetUserFolderAsync(long folderId, Guid userId)
    {
        return await _context.Folders
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
    }
}


