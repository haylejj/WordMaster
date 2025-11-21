using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class FolderService(
    IFolderRepository folderRepository,
    IWordRepository wordRepository,
    IWordFolderRepository wordFolderRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService
) : IFolderService
{
    private static readonly TimeSpan DropdownCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<ServiceResult<List<Folder>>> GetUserFoldersAsync(Guid userId)
    {
        List<Folder> folders = await folderRepository.GetUserFoldersAsync(userId);
        return ServiceResult<List<Folder>>.Success(folders);
    }

    public async Task<ServiceResult<Folder>> GetUserFolderAsync(long folderId, Guid userId)
    {
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        return folder == null
            ? ServiceResult<Folder>.Failure("Klasör bulunamadı.")
            : ServiceResult<Folder>.Success(folder);
    }

    public async Task<ServiceResult> AddFolderAsync(string name, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Failure("Klasör adı gereklidir.");
        }

        bool exists = await folderRepository.AnyAsync(f => f.UserId == userId && f.Name == name.Trim());
        if (exists)
        {
            return ServiceResult.Failure("Bu isimde bir klasör zaten mevcut.");
        }

        Folder folder = new()
        {
            Name = name.Trim(),
            UserId = userId,
            CreatedTime = DateTime.UtcNow
        };

        await folderRepository.AddAsync(folder);
        await unitOfWork.CommitAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateFolderAsync(long folderId, string name, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Failure("Klasör adı gereklidir.");
        }

        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return ServiceResult.Failure("Klasör bulunamadı.");
        }

        bool exists = await folderRepository.AnyAsync(f => f.UserId == userId && f.Name == name.Trim() && f.Id != folderId);
        if (exists)
        {
            return ServiceResult.Failure("Bu isimde bir klasör zaten mevcut.");
        }

        folder.Name = name.Trim();
        folderRepository.Update(folder);
        await unitOfWork.CommitAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteFolderAsync(long folderId, Guid userId)
    {
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return ServiceResult.Failure("Klasör bulunamadı.");
        }

        folderRepository.Remove(folder);
        await unitOfWork.CommitAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<List<Word>>> GetWordsInFolderAsync(long folderId, Guid userId)
    {
        // Ensure folder belongs to user
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return ServiceResult<List<Word>>.Failure("Klasör bulunamadı.");
        }
        List<Word> words = await wordFolderRepository.GetWordsInFolderAsync(folderId);
        return ServiceResult<List<Word>>.Success(words);
    }

    public async Task<ServiceResult> AddWordToFolderAsync(long folderId, long wordId, Guid userId)
    {
        bool folderExists = await folderRepository.AnyAsync(f => f.Id == folderId && f.UserId == userId);
        bool wordExists = await wordRepository.AnyAsync(w => w.Id == wordId && w.UserId == userId);
        if (!folderExists || !wordExists)
        {
            return ServiceResult.Failure("Klasör veya kelime bulunamadı.");
        }

        bool linkExists = await wordFolderRepository.LinkExistsAsync(folderId, wordId);
        if (linkExists)
        {
            return ServiceResult.Failure("Kelime zaten bu klasörde.");
        }

        await wordFolderRepository.AddAsync(new WordFolder { FolderId = folderId, WordId = wordId });
        await unitOfWork.CommitAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveWordFromFolderAsync(long folderId, long wordId, Guid userId)
    {
        bool folderExists = await folderRepository.AnyAsync(f => f.Id == folderId && f.UserId == userId);
        if (!folderExists)
        {
            return ServiceResult.Failure("Klasör bulunamadı.");
        }

        WordFolder? link = await wordFolderRepository.GetLinkAsync(folderId, wordId);
        if (link == null)
        {
            return ServiceResult.Failure("Kelime bu klasörde değil.");
        }

        wordFolderRepository.Remove(link);
        await unitOfWork.CommitAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<List<WordLookupDto>>> GetUserWordsAsync(Guid userId)
    {
        // Cache all user's words for dropdown usage; client will filter locally
        string cacheKey = $"dropdown_words:user:{userId}";
        List<WordLookupDto>? cached = await cacheService.GetAsync<List<WordLookupDto>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return ServiceResult<List<WordLookupDto>>.Success(cached);
        }

        List<WordLookupDto> words = await wordRepository
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.EnglishWord)
            .Select(x => new WordLookupDto
            {
                Id = x.Id,
                EnglishWord = x.EnglishWord
            })
            .AsNoTracking()
            .ToListAsync();

        await cacheService.SetAsync(cacheKey, words, DropdownCacheExpiration);
        return ServiceResult<List<WordLookupDto>>.Success(words);
    }
}


