using WordMaster.Application.Dto.Folder;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Requests.Folder;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFolderService
{
    Task<ServiceResult<List<FolderDto>>> GetUserFoldersAsync(Guid userId);
    Task<ServiceResult<FolderDto>> GetUserFolderAsync(long folderId, Guid userId);
    Task<ServiceResult> AddFolderAsync(CreateFolderRequest request, Guid userId);
    Task<ServiceResult> UpdateFolderAsync(long folderId, UpdateFolderRequest request, Guid userId);
    Task<ServiceResult> DeleteFolderAsync(long folderId, Guid userId);

    Task<ServiceResult<List<WordDto>>> GetWordsInFolderAsync(long folderId, Guid userId);
    Task<ServiceResult> AddWordToFolderAsync(AddWordToFolderRequest request, Guid userId);
    Task<ServiceResult> RemoveWordFromFolderAsync(AddWordToFolderRequest request, Guid userId);
}
