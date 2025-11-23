using System.Net;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Dto.Practice;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Word;
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

    public async Task<ServiceResult<WordDto>> GetWordForUserAsync(long id, Guid userId)
    {
        string cacheKey = $"word:{id}:user:{userId}";
        WordDto? cachedWordDto = await cacheService.GetAsync<WordDto>(cacheKey);
        if (cachedWordDto != null)
        {
            return ServiceResult<WordDto>.Success(cachedWordDto, HttpStatusCode.OK);
        }

        Word? word = await wordRepository.GetWordForUserAsync(id, userId);
        if (word == null)
        {
            return ServiceResult<WordDto>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        WordDto wordDto = new()
        {
            Id = word.Id,
            EnglishWord = word.EnglishWord,
            TurkishWord = word.TurkishWord
        };
        await cacheService.SetAsync(cacheKey, wordDto, GetByIdCacheExpiration);
        return ServiceResult<WordDto>.Success(wordDto, HttpStatusCode.OK);
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
        List<PracticeWordCacheDto>? cachedWords = await cacheService.GetAsync<List<PracticeWordCacheDto>>(cacheKey);

        List<PracticeWordCacheDto> practiceWords;
        if (cachedWords != null && cachedWords.Count > 0)
        {
            practiceWords = cachedWords;
        }
        else
        {
            List<Word> words = await wordRepository.GetWordsByUserAsync(userId);
            practiceWords = words.Select(w => new PracticeWordCacheDto
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

    public async Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord)
    {
        string? normalizedEnglishWord = englishWord?.NormalizeEnglishWord();

        Word? word = normalizedEnglishWord == null
            ? null
            : await wordRepository.GetWordByNormalizedEnglishAsync(userId, normalizedEnglishWord);

        if (word == null)
        {
            return ServiceResult<bool>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        string? normalizedTurkishWord = turkishWord?.NormalizeTurkishWord();
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

    public async Task<ServiceResult<PagedResult<WordDto>>> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize)
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

        List<WordDto> wordDtos = words.Select(w => new WordDto
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord,
            TurkishWord = w.TurkishWord
        }).ToList();

        PagedResult<WordDto> pagedResult = new()
        {
            Items = wordDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<WordDto>>.Success(pagedResult, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<List<WordLookupDto>>> GetUserWordsAsync(Guid userId)
    {
        // Cache all user's words for dropdown usage; client will filter locally
        string cacheKey = $"dropdown_words:user:{userId}";
        List<WordLookupDto>? cached = await cacheService.GetAsync<List<WordLookupDto>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return ServiceResult<List<WordLookupDto>>.Success(cached, HttpStatusCode.OK);
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

        await cacheService.SetAsync(cacheKey, words, TimeSpan.FromMinutes(10));
        return ServiceResult<List<WordLookupDto>>.Success(words, HttpStatusCode.OK);
    }
}
