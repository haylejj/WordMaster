using WordMaster.Application.Dto.Practice;
using WordMaster.Application.Dto.Unknows;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class UnknowsService(IUnknowsRepository unknowsRepository, IUnitOfWork unitOfWork, IWordService wordService, ICacheService cacheService) : IUnknowsService
{
    private static readonly Random _random = new();
    private static readonly TimeSpan PracticeCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan GetByIdCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<Result> DeleteUnknowsAsync(int unknowsId, string userId)
    {
        Unknows? unknow = await unknowsRepository.GetByIdForUserAsync(unknowsId, userId);
        if (unknow == null)
        {
            return Result.Failure("Bilinmeyen kelime bulunamadı veya size ait değil.");
        }

        unknowsRepository.Remove(unknow);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"unknows:{unknowsId}:user:{userId}");
        await cacheService.RemoveAsync($"unknows:user:{userId}");

        return Result.Success();
    }

    public async Task<Result<string>> GetRandomWordFromUnknowsAsync(string userId)
    {
        string cacheKey = $"unknows:user:{userId}";
        List<PracticeUnknowsCacheDto>? cachedUnknows = await cacheService.GetAsync<List<PracticeUnknowsCacheDto>>(cacheKey);

        List<PracticeUnknowsCacheDto> practiceUnknows;
        if (cachedUnknows != null && cachedUnknows.Count > 0)
        {
            practiceUnknows = cachedUnknows;
        }
        else
        {
            List<Unknows> unknows = await unknowsRepository.GetUserUnknowsWithWordAsync(userId);
            practiceUnknows = unknows.Select(u => new PracticeUnknowsCacheDto
            {
                EnglishWord = u.Word?.EnglishWord
            }).ToList();

            if (practiceUnknows.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, practiceUnknows, PracticeCacheExpiration);
            }
        }

        if (practiceUnknows.Count == 0)
        {
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, practiceUnknows.Count);
        return Result<string>.Success(practiceUnknows[index].EnglishWord ?? string.Empty);
    }

    public Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        return wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
    }

    public Task<List<Unknows>> GetUserUnknowsAsync(string userId)
    {
        return unknowsRepository.GetUserUnknowsWithWordAsync(userId);
    }

    public async Task<Result<bool>> ToggleUnknowsAsync(int wordId, string userId)
    {
        Result<Word> wordExists = await wordService.GetWordForUserAsync(wordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }

        Unknows? existingUnknow = await unknowsRepository.GetByWordForUserAsync(wordId, userId);
        if (existingUnknow == null)
        {
            Unknows unknow = new() { WordId = wordId, UserId = userId, CreatedTime = DateTime.Now };
            await unknowsRepository.AddAsync(unknow);
            await unitOfWork.CommitAsync();

            // Cache invalidation
            await cacheService.RemoveAsync($"unknows:user:{userId}");

            return Result<bool>.Success(true);
        }

        unknowsRepository.Remove(existingUnknow);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"unknows:user:{userId}");

        return Result<bool>.Success(false);
    }

    public async Task<Result<Unknows>> GetUnknowsWithWordAsync(int unknowsId, string userId)
    {
        string cacheKey = $"unknows:{unknowsId}:user:{userId}";
        UnknowsWithWordDto? cachedUnknowDto = await cacheService.GetAsync<UnknowsWithWordDto>(cacheKey);
        if (cachedUnknowDto != null)
        {
            Unknows cachedUnknow = new()
            {
                Id = cachedUnknowDto.Id,
                CreatedTime = cachedUnknowDto.CreatedTime,
                WordId = cachedUnknowDto.WordId,
                UserId = cachedUnknowDto.UserId,
                Word = cachedUnknowDto.Word != null ? new Word
                {
                    Id = cachedUnknowDto.Word.Id,
                    EnglishWord = cachedUnknowDto.Word.EnglishWord,
                    TurkishWord = cachedUnknowDto.Word.TurkishWord
                } : null
            };
            return Result<Unknows>.Success(cachedUnknow);
        }

        Unknows? unknow = await unknowsRepository.GetUnknowsWithWordAsync(unknowsId, userId);
        if (unknow == null)
        {
            return Result<Unknows>.Failure("Kayıt bulunamadı.");
        }

        var unknowDto = new UnknowsWithWordDto
        {
            Id = unknow.Id,
            CreatedTime = unknow.CreatedTime,
            WordId = unknow.WordId,
            UserId = unknow.UserId,
            Word = unknow.Word != null ? new WordDto
            {
                Id = unknow.Word.Id,
                EnglishWord = unknow.Word.EnglishWord,
                TurkishWord = unknow.Word.TurkishWord
            } : null
        };
        await cacheService.SetAsync(cacheKey, unknowDto, GetByIdCacheExpiration);
        return Result<Unknows>.Success(unknow);
    }

    public async Task<Result<(List<Unknows> Unknows, int TotalCount)>> GetPagedUnknowsAsync(string userId, string? search, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        (List<Unknows>? items, int totalCount) = await unknowsRepository.GetPagedUnknowsAsync(userId, search, page, pageSize);
        return Result<(List<Unknows> Unknows, int TotalCount)>.Success((items, totalCount));
    }
}

