using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using WordMaster.Application.Constants;
using WordMaster.Application.Key;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Practice;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Extensions;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class WordService(IWordRepository wordRepository, IUnitOfWork unitOfWork, ICacheService cacheService, ILogger<WordService> logger) : IWordService
{


    public async Task<ServiceResult<WordResponse>> GetWordForUserAsync(long id, Guid userId)
    {
        string cacheKey = CacheKeys.Word(id, userId);
        WordResponse? cachedWordDto = await cacheService.GetAsync<WordResponse>(cacheKey);
        if (cachedWordDto != null)
        {
            return ServiceResult<WordResponse>.Success(cachedWordDto, HttpStatusCode.OK);
        }

        Word? word = await wordRepository.GetWordForUserAsync(id, userId);
        if (word == null)
        {
            return ServiceResult<WordResponse>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        WordResponse wordDto = new()
        {
            Id = word.Id,
            EnglishWord = word.EnglishWord,
            TurkishWord = word.TurkishWord,
            FavoriteId = word.Favorite?.Id,
            UnknowsId = word.Unknows?.Id
        };
        await cacheService.SetAsync(cacheKey, wordDto, CacheDurations.Normal);
        return ServiceResult<WordResponse>.Success(wordDto, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> AddWordAsync(CreateWordRequest request, Guid userId)
    {
        request.EnglishWord = request.EnglishWord?.NormalizeEnglishWord()!;
        request.TurkishWord = request.TurkishWord?.NormalizeTurkishWord()!;

        if (string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return ServiceResult.Failure("İngilizce kelime boş olamaz.", HttpStatusCode.BadRequest);
        }

        ServiceResult<bool> isDuplicate = await IsWordDuplicateAsync(request.EnglishWord, userId);
        if (isDuplicate.Data == true)
        {
            return ServiceResult.Failure("Bu kelime zaten sözlüğünüzde mevcut.", HttpStatusCode.Conflict);
        }

        Word word = new()
        {
            EnglishWord = request.EnglishWord,
            TurkishWord = request.TurkishWord,
            UserId = userId,
            CreatedTime = DateTime.UtcNow
        };

        await wordRepository.AddAsync(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync(CacheKeys.Words(userId));
        await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));

        return ServiceResult.SuccessAsCreated();
    }

    public async Task<ServiceResult<WordResponse>> UpdateWordAsync(long wordId, UpdateWordRequest request, Guid userId)
    {
        Word? existingWord = await wordRepository.GetWordForUserTrackedWithFoldersAsync(wordId, userId);
        if (existingWord == null)
        {
            return ServiceResult<WordResponse>.Failure("Kelime bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        request.EnglishWord = request.EnglishWord?.NormalizeEnglishWord()!;
        request.TurkishWord = request.TurkishWord?.NormalizeTurkishWord()!;

        if (string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return ServiceResult<WordResponse>.Failure("İngilizce kelime boş olamaz.", HttpStatusCode.BadRequest);
        }

        bool isDuplicate = await wordRepository.AnyAsync(x =>
            x.Id != wordId &&
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord == request.EnglishWord);

        if (isDuplicate)
        {
            return ServiceResult<WordResponse>.Failure("Bu kelime zaten sözlüğünüzde mevcut.", HttpStatusCode.Conflict);
        }

        existingWord.EnglishWord = request.EnglishWord;
        existingWord.TurkishWord = request.TurkishWord;

        wordRepository.Update(existingWord);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
        await cacheService.RemoveAsync(CacheKeys.Words(userId));
        await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
        // Also invalidate practice lists that might contain this word
        await cacheService.RemoveAsync(CacheKeys.Favorites(userId));
        await cacheService.RemoveAsync(CacheKeys.Unknows(userId));

        // Invalidate folder caches
        if (existingWord.WordFolders != null)
        {
            foreach (var wordFolder in existingWord.WordFolders)
            {
                await cacheService.RemoveAsync(CacheKeys.FolderWords(wordFolder.FolderId, userId));
            }
        }

        WordResponse response = new()
        {
            Id = existingWord.Id,
            EnglishWord = existingWord.EnglishWord,
            TurkishWord = existingWord.TurkishWord
        };

        return ServiceResult<WordResponse>.Success(response, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> DeleteWordAsync(long wordId, Guid userId)
    {
        Word? word = await wordRepository.GetWordForUserTrackedWithFoldersAsync(wordId, userId);
        if (word == null)
        {
            return ServiceResult.Failure("Kelime bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        wordRepository.Remove(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
        await cacheService.RemoveAsync(CacheKeys.Words(userId));
        await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
        // Also invalidate practice lists that might contain this word
        await cacheService.RemoveAsync(CacheKeys.Favorites(userId));
        await cacheService.RemoveAsync(CacheKeys.Unknows(userId));

        // Invalidate folder caches
        if (word.WordFolders != null)
        {
            foreach (var wordFolder in word.WordFolders)
            {
                await cacheService.RemoveAsync(CacheKeys.FolderWords(wordFolder.FolderId, userId));
            }
        }

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult<bool>> IsWordDuplicateAsync(string englishWord, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(englishWord))
        {
            return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
        }

        string normalizedWord = englishWord.NormalizeEnglishWord();
        Word? existingWord = await wordRepository.GetWordByNormalizedEnglishAsync(userId, normalizedWord);

        return ServiceResult<bool>.Success(existingWord != null, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<PracticeWordResponse>> GetRandomWordAsync(Guid userId)
    {
        string cacheKey = CacheKeys.Words(userId);
        List<PracticeWordKey>? cachedWords = await cacheService.GetAsync<List<PracticeWordKey>>(cacheKey);

        List<PracticeWordKey> practiceWords;
        if (cachedWords != null && cachedWords.Count > 0)
        {
            practiceWords = cachedWords;
        }
        else
        {
            List<Word> words = await wordRepository.GetWordsByUserAsync(userId);
            practiceWords = words.Select(w => new PracticeWordKey
            {
                Id = w.Id,
                EnglishWord = w.EnglishWord,
                TurkishWord = w.TurkishWord
            }).ToList();

            if (practiceWords.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, practiceWords, CacheDurations.Practice);
            }
        }

        if (practiceWords.Count == 0)
        {
            return ServiceResult<PracticeWordResponse>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        int index = Random.Shared.Next(0, practiceWords.Count);
        PracticeWordKey randomWord = practiceWords[index];

        PracticeWordResponse response = new()
        {
            Id = randomWord.Id,
            EnglishWord = randomWord.EnglishWord ?? string.Empty,
            TurkishWord = randomWord.TurkishWord ?? string.Empty
        };

        return ServiceResult<PracticeWordResponse>.Success(response, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request)
    {
        Word? word = await wordRepository.GetWordForUserTrackedAsync(request.WordId, userId);

        if (word == null)
        {
            return ServiceResult<bool>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        string? normalizedAnswer = request.Answer?.NormalizeTurkishWord();
        bool isCorrect = word.TurkishWord == normalizedAnswer;

        word.IsLastAnswerCorrect = isCorrect;
        word.LastPracticeDate = DateTime.UtcNow;

        if (isCorrect)
        {
            word.ConsecutiveCorrectCount++;
            word.ConsecutiveWrongCount = 0;
            word.TotalCorrectCount++;
        }
        else
        {
            word.ConsecutiveWrongCount++;
            word.ConsecutiveCorrectCount = 0;
            word.TotalWrongCount++;
        }

        wordRepository.Update(word);
        await unitOfWork.CommitAsync();

        return ServiceResult<bool>.Success(isCorrect, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<PagedResult<WordResponse>>> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        (List<Word>? words, int totalCount) = await wordRepository.GetPagedWordsAsync(userId, search, page, pageSize);

        List<WordResponse> wordDtos = words.Select(w => new WordResponse
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord,
            TurkishWord = w.TurkishWord,
            FavoriteId = w.Favorite?.Id,
            UnknowsId = w.Unknows?.Id
        }).ToList();

        PagedResult<WordResponse> pagedResult = new()
        {
            Items = wordDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<WordResponse>>.Success(pagedResult, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<List<WordLookupResponse>>> GetUserWordsAsync(Guid userId)
    {
        // Cache all user's words for dropdown usage; client will filter locally
        string cacheKey = CacheKeys.UserWordsDropdown(userId);
        List<WordLookupResponse>? cached = await cacheService.GetAsync<List<WordLookupResponse>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return ServiceResult<List<WordLookupResponse>>.Success(cached, HttpStatusCode.OK);
        }

        List<WordLookupResponse> words = await wordRepository
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.EnglishWord)
            .Select(x => new WordLookupResponse
            {
                Id = x.Id,
                EnglishWord = x.EnglishWord
            })
            .AsNoTracking()
            .ToListAsync();

        await cacheService.SetAsync(cacheKey, words, CacheDurations.Normal);
        return ServiceResult<List<WordLookupResponse>>.Success(words, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<bool>> BulkUpdateStatsAsync(Guid userId, BulkUpdateStatsRequest request)
    {
        if (request.Results == null || request.Results.Count == 0)
        {
            return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
        }

        List<long> wordIds = request.Results.Select(r => (long)r.WordId).ToList();
        List<Word> words = await wordRepository
            .Where(x => wordIds.Contains(x.Id) && x.UserId == userId)
            .ToListAsync();

        int updatedCount = 0;
        foreach (PracticeResultItem result in request.Results)
        {
            Word? word = words.FirstOrDefault(w => w.Id == result.WordId);
            if (word == null)
            {
                continue;
            }

            word.IsLastAnswerCorrect = result.IsCorrect;
            word.LastPracticeDate = DateTime.UtcNow;

            if (result.IsCorrect)
            {
                word.ConsecutiveCorrectCount++;
                word.ConsecutiveWrongCount = 0;
                word.TotalCorrectCount++;
            }
            else
            {
                word.ConsecutiveWrongCount++;
                word.ConsecutiveCorrectCount = 0;
                word.TotalWrongCount++;
            }

            wordRepository.Update(word);
            updatedCount++;
        }

        await unitOfWork.CommitAsync();

        logger.LogInformation("Practice completed for user {UserId}. Updated stats for {UpdatedCount} words.", userId, updatedCount);

        await cacheService.RemoveAsync(CacheKeys.Words(userId));
        foreach (long id in wordIds)
        {
            await cacheService.RemoveAsync(CacheKeys.Word(id, userId));
        }

        return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
    }


    // Admin Methods
    public async Task<ServiceResult<PagedResult<AdminWordResponse>>> GetAdminPagedWordsAsync(string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 10;

        (List<Word> words, int totalCount) = await wordRepository.GetAdminPagedWordsAsync(search, page, pageSize);

        List<AdminWordResponse> wordDtos = words.Select(w => new AdminWordResponse
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord,
            TurkishWord = w.TurkishWord,
            UserId = w.UserId,
            UserName = w.User?.UserName,
            CreatedTime = w.CreatedTime
        }).ToList();

        PagedResult<AdminWordResponse> pagedResult = new()
        {
            Items = wordDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<AdminWordResponse>>.Success(pagedResult, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<AdminWordResponse>> AdminUpdateWordAsync(long wordId, UpdateWordRequest request)
    {
        Word? existingWord = await wordRepository.GetByIdAsync((int)wordId);
        if (existingWord == null)
        {
            return ServiceResult<AdminWordResponse>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        request.EnglishWord = request.EnglishWord?.NormalizeEnglishWord()!;
        request.TurkishWord = request.TurkishWord?.NormalizeTurkishWord()!;

        if (string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return ServiceResult<AdminWordResponse>.Failure("İngilizce kelime boş olamaz.", HttpStatusCode.BadRequest);
        }

        // Admin can update any word, duplicate check might be tricky across users, 
        // but usually we check duplicates for the SAME user.
        // Let's check if the user already has this word (excluding the current one)
        if (existingWord.UserId.HasValue)
        {
            bool isDuplicate = await wordRepository.AnyAsync(x =>
               x.Id != wordId &&
               x.UserId == existingWord.UserId &&
               x.EnglishWord != null &&
               x.EnglishWord == request.EnglishWord);

            if (isDuplicate)
            {
                return ServiceResult<AdminWordResponse>.Failure("Bu kullanıcıda bu kelime zaten mevcut.", HttpStatusCode.Conflict);
            }
        }

        existingWord.EnglishWord = request.EnglishWord;
        existingWord.TurkishWord = request.TurkishWord;

        wordRepository.Update(existingWord);
        await unitOfWork.CommitAsync();

        // Cache invalidation if user exists
        if (existingWord.UserId.HasValue)
        {
            Guid userId = existingWord.UserId.Value;
            await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
            await cacheService.RemoveAsync(CacheKeys.Words(userId));
            await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
        }

        // We need to fetch the user again or assume it's loaded if we want to return UserName
        // But GetByIdAsync might not include User. 
        // For simplicity, we return the response without UserName or fetch it if needed.
        // Let's return basic info.

        AdminWordResponse response = new()
        {
            Id = existingWord.Id,
            EnglishWord = existingWord.EnglishWord,
            TurkishWord = existingWord.TurkishWord,
            UserId = existingWord.UserId,
            CreatedTime = existingWord.CreatedTime
        };

        return ServiceResult<AdminWordResponse>.Success(response, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> AdminDeleteWordAsync(long wordId)
    {
        Word? word = await wordRepository.GetByIdAsync((int)wordId);
        if (word == null)
        {
            return ServiceResult.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        wordRepository.Remove(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        if (word.UserId.HasValue)
        {
            Guid userId = word.UserId.Value;
            await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
            await cacheService.RemoveAsync(CacheKeys.Words(userId));
            await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
            await cacheService.RemoveAsync(CacheKeys.Favorites(userId));
            await cacheService.RemoveAsync(CacheKeys.Unknows(userId));
        }

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
}
