using WordMaster.Domain.Entities;
namespace WordMaster.Application.Persistence.Repositories;

public interface IFolderRepository : IGenericRepository<Folder>
{
    Task<List<Folder>> GetUserFoldersAsync(string userId);
    Task<Folder?> GetUserFolderAsync(int folderId, string userId);
}


