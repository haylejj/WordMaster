using System.Net;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Folder;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Folder;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

using WordMaster.Application.Constants;

public class FolderService(
    IFolderRepository folderRepository,
    IWordRepository wordRepository,
    IWordFolderRepository wordFolderRepository,
    IUnitOfWork unitOfWork,
    IWordService wordService,
    ICacheService cacheService
) : IFolderService
{
    public async Task<ServiceResult<List<FolderResponse>>> GetUserFoldersAsync(Guid userId)
    {
        string cacheKey = CacheKeys.Folders(userId);
        List<FolderResponse>? cachedFolders = await cacheService.GetAsync<List<FolderResponse>>(cacheKey);
        if (cachedFolders != null)
        {
            return ServiceResult<List<FolderResponse>>.Success(cachedFolders, HttpStatusCode.OK);
        }

        List<Folder> folders = await folderRepository.GetUserFoldersAsync(userId);
        List<FolderResponse> folderDtos = folders.Select(f => new FolderResponse
        {
            Id = f.Id,
            Name = f.Name,
            CreatedTime = f.CreatedTime,
            WordCount = f.WordFolders.Count
        }).ToList();

        await cacheService.SetAsync(cacheKey, folderDtos, CacheDurations.Normal);
        return ServiceResult<List<FolderResponse>>.Success(folderDtos, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<FolderResponse>> GetUserFolderAsync(long folderId, Guid userId)
    {
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return ServiceResult<FolderResponse>.Failure("Klasör bulunamadı.", HttpStatusCode.NotFound);
        }

        FolderResponse folderDto = new()
        {
            Id = folder.Id,
            Name = folder.Name,
            CreatedTime = folder.CreatedTime,
            WordCount = folder.WordFolders.Count
        };

        return ServiceResult<FolderResponse>.Success(folderDto, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<FolderResponse>> AddFolderAsync(CreateFolderRequest request, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ServiceResult<FolderResponse>.Failure("Klasör adı gereklidir.", HttpStatusCode.BadRequest);
        }

        bool exists = await folderRepository.AnyAsync(f => f.UserId == userId && f.Name == request.Name.Trim());
        if (exists)
        {
            return ServiceResult<FolderResponse>.Failure("Bu isimde bir klasör zaten mevcut.", HttpStatusCode.Conflict);
        }

        Folder folder = new()
        {
            Name = request.Name.Trim(),
            UserId = userId,
            CreatedTime = DateTime.UtcNow
        };

        await folderRepository.AddAsync(folder);
        await unitOfWork.CommitAsync();

        FolderResponse response = new()
        {
            Id = folder.Id,
            Name = folder.Name,
            CreatedTime = folder.CreatedTime,
            WordCount = 0
        };

        await cacheService.RemoveAsync(CacheKeys.Folders(userId));

        return ServiceResult<FolderResponse>.SuccessAsCreated(response, null);
    }
    public async Task<ServiceResult<FolderResponse>> UpdateFolderAsync(long folderId, UpdateFolderRequest request, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ServiceResult<FolderResponse>.Failure("Klasör adı gereklidir.", HttpStatusCode.BadRequest);
        }

        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return ServiceResult<FolderResponse>.Failure("Klasör bulunamadı.", HttpStatusCode.NotFound);
        }

        bool exists = await folderRepository.AnyAsync(f => f.UserId == userId && f.Name == request.Name.Trim() && f.Id != folderId);
        if (exists)
        {
            return ServiceResult<FolderResponse>.Failure("Bu isimde bir klasör zaten mevcut.", HttpStatusCode.Conflict);
        }

        folder.Name = request.Name.Trim();
        folderRepository.Update(folder);
        await unitOfWork.CommitAsync();

        FolderResponse response = new()
        {
            Id = folder.Id,
            Name = folder.Name,
            CreatedTime = folder.CreatedTime,
            WordCount = folder.WordFolders.Count
        };

        await cacheService.RemoveAsync(CacheKeys.Folders(userId));

        return ServiceResult<FolderResponse>.Success(response, HttpStatusCode.OK);
    }
    public async Task<ServiceResult> DeleteFolderAsync(long folderId, Guid userId)
    {
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return ServiceResult.Failure("Klasör bulunamadı.", HttpStatusCode.NotFound);
        }

        folderRepository.Remove(folder);
        await unitOfWork.CommitAsync();
        await cacheService.RemoveAsync(CacheKeys.Folders(userId));
        await cacheService.RemoveAsync(CacheKeys.FolderWords(folderId, userId));

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult<List<FolderWordResponse>>> GetWordsInFolderAsync(long folderId, Guid userId)
    {
        // Ensure folder belongs to user
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return ServiceResult<List<FolderWordResponse>>.Failure("Klasör bulunamadı.", HttpStatusCode.NotFound);
        }
        string cacheKey = CacheKeys.FolderWords(folderId, userId);
        List<FolderWordResponse>? cachedWords = await cacheService.GetAsync<List<FolderWordResponse>>(cacheKey);
        if (cachedWords != null)
        {
            return ServiceResult<List<FolderWordResponse>>.Success(cachedWords, HttpStatusCode.OK);
        }

        List<Word> words = await wordFolderRepository.GetWordsInFolderAsync(folderId);
        List<FolderWordResponse> wordDtos = words.Select(w => new FolderWordResponse
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord ?? string.Empty,
            TurkishWord = w.TurkishWord ?? string.Empty
        }).ToList();

        await cacheService.SetAsync(cacheKey, wordDtos, CacheDurations.Normal);
        return ServiceResult<List<FolderWordResponse>>.Success(wordDtos, HttpStatusCode.OK);
    }
    public async Task<ServiceResult> AddWordToFolderAsync(AddWordToFolderRequest request, Guid userId)
    {
        bool folderExists = await folderRepository.AnyAsync(f => f.Id == request.FolderId && f.UserId == userId);
        bool wordExists = await wordRepository.AnyAsync(w => w.Id == request.WordId && w.UserId == userId);
        if (!folderExists || !wordExists)
        {
            return ServiceResult.Failure("Klasör veya kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        bool linkExists = await wordFolderRepository.LinkExistsAsync(request.FolderId, request.WordId);
        if (linkExists)
        {
            return ServiceResult.Failure("Kelime zaten bu klasörde.", HttpStatusCode.Conflict);
        }

        await wordFolderRepository.AddAsync(new WordFolder { FolderId = request.FolderId, WordId = request.WordId });
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(CacheKeys.FolderWords(request.FolderId, userId));
        await cacheService.RemoveAsync(CacheKeys.Folders(userId));

        return ServiceResult.SuccessAsCreated();
    }
    public async Task<ServiceResult> RemoveWordFromFolderAsync(AddWordToFolderRequest request, Guid userId)
    {
        bool folderExists = await folderRepository.AnyAsync(f => f.Id == request.FolderId && f.UserId == userId);
        if (!folderExists)
        {
            return ServiceResult.Failure("Klasör bulunamadı.", HttpStatusCode.NotFound);
        }

        WordFolder? link = await wordFolderRepository.GetLinkAsync(request.FolderId, request.WordId);
        if (link == null)
        {
            return ServiceResult.Failure("Kelime bu klasörde değil.", HttpStatusCode.NotFound);
        }

        wordFolderRepository.Remove(link);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(CacheKeys.FolderWords(request.FolderId, userId));
        await cacheService.RemoveAsync(CacheKeys.Folders(userId));

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
    public Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request)
    {
        return wordService.CheckTranslationAndUpdateAsync(userId, request);
    }
}
