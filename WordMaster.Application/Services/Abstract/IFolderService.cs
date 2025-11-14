using WordMaster.Application.Dto.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFolderService
{
    Task<Result<List<Folder>>> GetUserFoldersAsync(string userId);
    Task<Result<Folder>> GetUserFolderAsync(int folderId, string userId);
    Task<Result> AddFolderAsync(string name, string userId);
    Task<Result> UpdateFolderAsync(int folderId, string name, string userId);
    Task<Result> DeleteFolderAsync(int folderId, string userId);

    Task<Result<List<Word>>> GetWordsInFolderAsync(int folderId, string userId);
    Task<Result> AddWordToFolderAsync(int folderId, int wordId, string userId);
    Task<Result> RemoveWordFromFolderAsync(int folderId, int wordId, string userId);

    Task<Result<List<WordLookupDto>>> GetUserWordsAsync(string userId);
}


