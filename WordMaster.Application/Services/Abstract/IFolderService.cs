using WordMaster.Application.Requests.Folder;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Folder;
using WordMaster.Application.Responses.Word;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFolderService
{
    Task<ServiceResult<List<FolderResponse>>> GetUserFoldersAsync(Guid userId);
    Task<ServiceResult<FolderResponse>> GetUserFolderAsync(long folderId, Guid userId);
    Task<ServiceResult> AddFolderAsync(CreateFolderRequest request, Guid userId);
    Task<ServiceResult> UpdateFolderAsync(long folderId, UpdateFolderRequest request, Guid userId);
    Task<ServiceResult> DeleteFolderAsync(long folderId, Guid userId);
    Task<ServiceResult<List<WordResponse>>> GetWordsInFolderAsync(long folderId, Guid userId);
    Task<ServiceResult> AddWordToFolderAsync(AddWordToFolderRequest request, Guid userId);
    Task<ServiceResult> RemoveWordFromFolderAsync(AddWordToFolderRequest request, Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request);
}
