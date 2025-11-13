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

    public async Task<Result<List<Folder>>> GetUserFoldersAsync(string userId)
    {
        List<Folder> folders = await folderRepository.GetUserFoldersAsync(userId);
        return Result<List<Folder>>.Success(folders);
    }

    public async Task<Result<Folder>> GetUserFolderAsync(int folderId, string userId)
    {
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        return folder == null
            ? Result<Folder>.Failure("Klasör bulunamadı.")
            : Result<Folder>.Success(folder);
    }

    public async Task<Result> AddFolderAsync(string name, string userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure("Klasör adı gereklidir.");
        }

        bool exists = await folderRepository.AnyAsync(f => f.UserId == userId && f.Name == name.Trim());
        if (exists)
        {
            return Result.Failure("Bu isimde bir klasör zaten mevcut.");
        }

        Folder folder = new()
        {
            Name = name.Trim(),
            UserId = userId,
            CreatedTime = DateTime.UtcNow
        };

        await folderRepository.AddAsync(folder);
        await unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<List<Word>>> GetWordsInFolderAsync(int folderId, string userId)
    {
        // Ensure folder belongs to user
        Folder? folder = await folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return Result<List<Word>>.Failure("Klasör bulunamadı.");
        }
        List<Word> words = await wordFolderRepository.GetWordsInFolderAsync(folderId);
        return Result<List<Word>>.Success(words);
    }

    public async Task<Result> AddWordToFolderAsync(int folderId, int wordId, string userId)
    {
        bool folderExists = await folderRepository.AnyAsync(f => f.Id == folderId && f.UserId == userId);
        bool wordExists = await wordRepository.AnyAsync(w => w.Id == wordId && w.UserId == userId);
        if (!folderExists || !wordExists)
        {
            return Result.Failure("Klasör veya kelime bulunamadı.");
        }

        bool linkExists = await wordFolderRepository.LinkExistsAsync(folderId, wordId);
        if (linkExists)
        {
            return Result.Failure("Kelime zaten bu klasörde.");
        }

        await wordFolderRepository.AddAsync(new WordFolder { FolderId = folderId, WordId = wordId });
        await unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveWordFromFolderAsync(int folderId, int wordId, string userId)
    {
        bool folderExists = await folderRepository.AnyAsync(f => f.Id == folderId && f.UserId == userId);
        if (!folderExists)
        {
            return Result.Failure("Klasör bulunamadı.");
        }

        WordFolder? link = await wordFolderRepository.GetLinkAsync(folderId, wordId);
        if (link == null)
        {
            return Result.Failure("Kelime bu klasörde değil.");
        }

        wordFolderRepository.Remove(link);
        await unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<List<WordLookupDto>>> GetUserWordsAsync(string userId)
    {
        // Cache all user's words for dropdown usage; client will filter locally
        string cacheKey = $"dropdown_words:user:{userId}";
        List<WordLookupDto>? cached = await cacheService.GetAsync<List<WordLookupDto>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return Result<List<WordLookupDto>>.Success(cached);
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
        return Result<List<WordLookupDto>>.Success(words);
    }
}


