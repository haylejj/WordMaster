using WordMaster.Domain.Entities;
namespace WordMaster.Application.Persistence.Repositories;

public interface IFolderRepository : IGenericRepository<Folder>
{
    Task<List<Folder>> GetUserFoldersAsync(Guid userId);
    Task<Folder?> GetUserFolderAsync(long folderId, Guid userId);
}


