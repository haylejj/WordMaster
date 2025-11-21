using WordMaster.Application.Dto.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFolderService
{
    Task<Result<List<Folder>>> GetUserFoldersAsync(Guid userId);
    Task<Result<Folder>> GetUserFolderAsync(long folderId, Guid userId);
    Task<Result> AddFolderAsync(string name, Guid userId);
    Task<Result> UpdateFolderAsync(long folderId, string name, Guid userId);
    Task<Result> DeleteFolderAsync(long folderId, Guid userId);

    Task<Result<List<Word>>> GetWordsInFolderAsync(long folderId, Guid userId);
    Task<Result> AddWordToFolderAsync(long folderId, long wordId, Guid userId);
    Task<Result> RemoveWordFromFolderAsync(long folderId, long wordId, Guid userId);

    Task<Result<List<WordLookupDto>>> GetUserWordsAsync(Guid userId);
}


