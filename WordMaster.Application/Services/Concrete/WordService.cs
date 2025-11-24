using Microsoft.EntityFrameworkCore;
using System.Net;
using WordMaster.Application.Key;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Extensions;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class WordService(IWordRepository wordRepository, IUnitOfWork unitOfWork, ICacheService cacheService) : IWordService
{
    private static readonly Random _random = new();
    private static readonly TimeSpan PracticeCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan GetByIdCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<ServiceResult<WordResponse>> GetWordForUserAsync(long id, Guid userId)
    {
        string cacheKey = $"word:{id}:user:{userId}";
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
        await cacheService.SetAsync(cacheKey, wordDto, GetByIdCacheExpiration);
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
        await cacheService.RemoveAsync($"words:user:{userId}");

        return ServiceResult.SuccessAsCreated();
    }

    public async Task<ServiceResult> UpdateWordAsync(long wordId, UpdateWordRequest request, Guid userId)
    {
        Word? existingWord = await wordRepository.GetWordForUserTrackedAsync(wordId, userId);
        if (existingWord == null)
        {
            return ServiceResult.Failure("Kelime bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        request.EnglishWord = request.EnglishWord?.NormalizeEnglishWord()!;
        request.TurkishWord = request.TurkishWord?.NormalizeTurkishWord()!;

        if (string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return ServiceResult.Failure("İngilizce kelime boş olamaz.", HttpStatusCode.BadRequest);
        }

        bool isDuplicate = await wordRepository.AnyAsync(x =>
            x.Id != wordId &&
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord == request.EnglishWord);

        if (isDuplicate)
        {
            return ServiceResult.Failure("Bu kelime zaten sözlüğünüzde mevcut.", HttpStatusCode.Conflict);
        }

        existingWord.EnglishWord = request.EnglishWord;
        existingWord.TurkishWord = request.TurkishWord;

        wordRepository.Update(existingWord);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"word:{wordId}:user:{userId}");
        await cacheService.RemoveAsync($"words:user:{userId}");

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult> DeleteWordAsync(long wordId, Guid userId)
    {
        Word? word = await wordRepository.GetWordForUserTrackedAsync(wordId, userId);
        if (word == null)
        {
            return ServiceResult.Failure("Kelime bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        wordRepository.Remove(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"word:{wordId}:user:{userId}");
        await cacheService.RemoveAsync($"words:user:{userId}");

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

    public async Task<ServiceResult<string>> GetRandomWordAsync(Guid userId)
    {
        string cacheKey = $"words:user:{userId}";
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
                EnglishWord = w.EnglishWord
            }).ToList();

            if (practiceWords.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, practiceWords, PracticeCacheExpiration);
            }
        }

        if (practiceWords.Count == 0)
        {
            return ServiceResult<string>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        int index = _random.Next(0, practiceWords.Count);
        return ServiceResult<string>.Success(practiceWords[index].EnglishWord ?? string.Empty, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request)
    {
        string? normalizedEnglishWord = request.EnglishWord?.NormalizeEnglishWord();

        Word? word = normalizedEnglishWord == null
            ? null
            : await wordRepository.GetWordByNormalizedEnglishAsync(userId, normalizedEnglishWord);

        if (word == null)
        {
            return ServiceResult<bool>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        string? normalizedTurkishWord = request.TurkishWord?.NormalizeTurkishWord();
        bool isCorrect = word.TurkishWord == normalizedTurkishWord;

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

        // Cache invalidation - kelime güncellendiği için cache'i temizle
        await cacheService.RemoveAsync($"word:{word.Id}:user:{userId}");
        await cacheService.RemoveAsync($"words:user:{userId}");

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
        string cacheKey = $"dropdown_words:user:{userId}";
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

        await cacheService.SetAsync(cacheKey, words, TimeSpan.FromMinutes(10));
        return ServiceResult<List<WordLookupResponse>>.Success(words, HttpStatusCode.OK);
    }
}
