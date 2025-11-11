using Microsoft.EntityFrameworkCore;
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
    private readonly IFolderRepository _folderRepository = folderRepository;
    private readonly IWordRepository _wordRepository = wordRepository;
    private readonly IWordFolderRepository _wordFolderRepository = wordFolderRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private static readonly TimeSpan DropdownCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<Result<List<Folder>>> GetUserFoldersAsync(string userId)
    {
        var folders = await _folderRepository.GetUserFoldersAsync(userId);
        return Result<List<Folder>>.Success(folders);
    }

    public async Task<Result<Folder>> GetUserFolderAsync(int folderId, string userId)
    {
        var folder = await _folderRepository.GetUserFolderAsync(folderId, userId);
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

        var exists = await _folderRepository.AnyAsync(f => f.UserId == userId && f.Name == name.Trim());
        if (exists)
        {
            return Result.Failure("Bu isimde bir klasör zaten mevcut.");
        }

        var folder = new Folder
        {
            Name = name.Trim(),
            UserId = userId,
            CreatedTime = DateTime.UtcNow
        };

        await _folderRepository.AddAsync(folder);
        await _unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<List<Word>>> GetWordsInFolderAsync(int folderId, string userId)
    {
        // Ensure folder belongs to user
        var folder = await _folderRepository.GetUserFolderAsync(folderId, userId);
        if (folder == null)
        {
            return Result<List<Word>>.Failure("Klasör bulunamadı.");
        }
        var words = await _wordFolderRepository.GetWordsInFolderAsync(folderId);
        return Result<List<Word>>.Success(words);
    }

    public async Task<Result> AddWordToFolderAsync(int folderId, int wordId, string userId)
    {
        var folderExists = await _folderRepository.AnyAsync(f => f.Id == folderId && f.UserId == userId);
        var wordExists = await _wordRepository.AnyAsync(w => w.Id == wordId && w.UserId == userId);
        if (!folderExists || !wordExists)
        {
            return Result.Failure("Klasör veya kelime bulunamadı.");
        }

        var linkExists = await _wordFolderRepository.LinkExistsAsync(folderId, wordId);
        if (linkExists)
        {
            return Result.Failure("Kelime zaten bu klasörde.");
        }

        await _wordFolderRepository.AddAsync(new WordFolder { FolderId = folderId, WordId = wordId });
        await _unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveWordFromFolderAsync(int folderId, int wordId, string userId)
    {
        var folderExists = await _folderRepository.AnyAsync(f => f.Id == folderId && f.UserId == userId);
        if (!folderExists)
        {
            return Result.Failure("Klasör bulunamadı.");
        }

        var link = await _wordFolderRepository.GetLinkAsync(folderId, wordId);
        if (link == null)
        {
            return Result.Failure("Kelime bu klasörde değil.");
        }

        _wordFolderRepository.Remove(link);
        await _unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<List<Word>>> GetUserWordsAsync(string userId, string? search, int take)
    {
        // Cache all user's words for dropdown usage; client will filter locally
        var cacheKey = $"dropdown_words:user:{userId}";
        var cached = await _cacheService.GetAsync<List<Word>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return Result<List<Word>>.Success(cached);
        }

        var words = await _wordRepository
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.EnglishWord)
            .Select(x => new Word
            {
                Id = x.Id,
                EnglishWord = x.EnglishWord
            })
            .AsNoTracking()
            .ToListAsync();

        await _cacheService.SetAsync(cacheKey, words, DropdownCacheExpiration);
        return Result<List<Word>>.Success(words);
    }
}


