using WordMaster.Application.Dto.Practice;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
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

    public async Task<Result<Word>> GetWordForUserAsync(int id, string userId)
    {
        string cacheKey = $"word:{id}:user:{userId}";
        var cachedWordDto = await cacheService.GetAsync<WordDto>(cacheKey);
        if (cachedWordDto != null)
        {
            Word cachedWord = new()
            {
                Id = cachedWordDto.Id,
                EnglishWord = cachedWordDto.EnglishWord,
                TurkishWord = cachedWordDto.TurkishWord
            };
            return Result<Word>.Success(cachedWord);
        }

        Word? word = await wordRepository.GetWordForUserAsync(id, userId);
        if (word == null)
        {
            return Result<Word>.Failure("Kelime bulunamadı.");
        }

        var wordDto = new WordDto
        {
            Id = word.Id,
            EnglishWord = word.EnglishWord,
            TurkishWord = word.TurkishWord
        };
        await cacheService.SetAsync(cacheKey, wordDto, GetByIdCacheExpiration);
        return Result<Word>.Success(word);
    }

    public async Task<Result> AddWordAsync(WordDto wordDto, string userId)
    {
        wordDto.EnglishWord = wordDto.EnglishWord?.NormalizeEnglishWord();
        wordDto.TurkishWord = wordDto.TurkishWord?.NormalizeTurkishWord();

        if (string.IsNullOrWhiteSpace(wordDto.EnglishWord))
        {
            return Result.Failure("İngilizce kelime boş olamaz.");
        }

        var isDuplicate = await IsWordDuplicateAsync(wordDto.EnglishWord, userId);
        if (isDuplicate.Data == true)
        {
            return Result.Failure("Bu kelime zaten sözlüğünüzde mevcut.");
        }

        Word word = new()
        {
            EnglishWord = wordDto.EnglishWord,
            TurkishWord = wordDto.TurkishWord,
            UserId = userId,
            CreatedTime = DateTime.Now
        };

        await wordRepository.AddAsync(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"words:user:{userId}");

        return Result.Success();
    }

    public async Task<Result> UpdateWordAsync(int wordId, WordDto wordDto, string userId)
    {
        Word? existingWord = await wordRepository.GetWordForUserTrackedAsync(wordId, userId);
        if (existingWord == null)
        {
            return Result.Failure("Kelime bulunamadı veya size ait değil.");
        }

        wordDto.EnglishWord = wordDto.EnglishWord?.NormalizeEnglishWord();
        wordDto.TurkishWord = wordDto.TurkishWord?.NormalizeTurkishWord();

        if (string.IsNullOrWhiteSpace(wordDto.EnglishWord))
        {
            return Result.Failure("İngilizce kelime boş olamaz.");
        }

        bool isDuplicate = await wordRepository.AnyAsync(x =>
            x.Id != wordId &&
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord == wordDto.EnglishWord);

        if (isDuplicate)
        {
            return Result.Failure("Bu kelime zaten sözlüğünüzde mevcut.");
        }

        existingWord.EnglishWord = wordDto.EnglishWord;
        existingWord.TurkishWord = wordDto.TurkishWord;

        wordRepository.Update(existingWord);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"word:{wordId}:user:{userId}");
        await cacheService.RemoveAsync($"words:user:{userId}");

        return Result.Success();
    }

    public async Task<Result> DeleteWordAsync(int wordId, string userId)
    {
        Word? word = await wordRepository.GetWordForUserTrackedAsync(wordId, userId);
        if (word == null)
        {
            return Result.Failure("Kelime bulunamadı veya size ait değil.");
        }

        wordRepository.Remove(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"word:{wordId}:user:{userId}");
        await cacheService.RemoveAsync($"words:user:{userId}");

        return Result.Success();
    }

    public async Task<Result<bool>> IsWordDuplicateAsync(string englishWord, string userId)
    {
        if (string.IsNullOrWhiteSpace(englishWord))
        {
            return Result<bool>.Success(false);
        }

        string normalizedWord = englishWord.NormalizeEnglishWord();
        Word? existingWord = await wordRepository.GetWordByNormalizedEnglishAsync(userId, normalizedWord);

        return Result<bool>.Success(existingWord != null);
    }

    public async Task<Result<string>> GetRandomWordAsync(string userId)
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
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, practiceWords.Count);
        return Result<string>.Success(practiceWords[index].EnglishWord ?? string.Empty);
    }

    public async Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        string? normalizedEnglishWord = englishWord?.NormalizeEnglishWord();

        Word? word = normalizedEnglishWord == null
            ? null
            : await wordRepository.GetWordByNormalizedEnglishAsync(userId, normalizedEnglishWord);

        if (word == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }

        string? normalizedTurkishWord = turkishWord?.NormalizeTurkishWord();
        bool isCorrect = word.TurkishWord == normalizedTurkishWord;

        word.IsLastAnswerCorrect = isCorrect;
        word.LastPracticeDate = DateTime.Now;

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

        return Result<bool>.Success(isCorrect);
    }

    public async Task<Result<(List<Word> Words, int TotalCount)>> GetPagedWordsAsync(string userId, string? search, int page, int pageSize)
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
        return Result<(List<Word> Words, int TotalCount)>.Success((words, totalCount));
    }
}

