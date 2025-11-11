using WordMaster.Domain.Entities;
namespace WordMaster.Application.Persistence.Repositories;

public interface IWordFolderRepository : IGenericRepository<WordFolder>
{
    Task<List<Word>> GetWordsInFolderAsync(int folderId);
    Task<bool> LinkExistsAsync(int folderId, int wordId);
    Task<WordFolder?> GetLinkAsync(int folderId, int wordId);
}


