using WordMaster.Application.Dto.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFolderService
{
    Task<ServiceResult<List<Folder>>> GetUserFoldersAsync(Guid userId);
    Task<ServiceResult<Folder>> GetUserFolderAsync(long folderId, Guid userId);
    Task<ServiceResult> AddFolderAsync(string name, Guid userId);
    Task<ServiceResult> UpdateFolderAsync(long folderId, string name, Guid userId);
    Task<ServiceResult> DeleteFolderAsync(long folderId, Guid userId);

    Task<ServiceResult<List<Word>>> GetWordsInFolderAsync(long folderId, Guid userId);
    Task<ServiceResult> AddWordToFolderAsync(long folderId, long wordId, Guid userId);
    Task<ServiceResult> RemoveWordFromFolderAsync(long folderId, long wordId, Guid userId);

    Task<ServiceResult<List<WordLookupDto>>> GetUserWordsAsync(Guid userId);
}


