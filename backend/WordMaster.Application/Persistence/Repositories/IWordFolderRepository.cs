using WordMaster.Domain.Entities;
namespace WordMaster.Application.Persistence.Repositories;

public interface IWordFolderRepository : IGenericRepository<WordFolder>
{
    Task<List<Word>> GetWordsInFolderAsync(long folderId);
    Task<bool> LinkExistsAsync(long folderId, long wordId);
    Task<WordFolder?> GetLinkAsync(long folderId, long wordId);
}


