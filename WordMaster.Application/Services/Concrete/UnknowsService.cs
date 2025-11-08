using WordMaster.Application.Dto;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class UnknowsService(IUnknowsRepository unknowsRepository, IUnitOfWork unitOfWork, IWordService wordService, ICacheService cacheService) : IUnknowsService
{
    private static readonly Random _random = new();
    private readonly IUnknowsRepository _unknowsRepository = unknowsRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IWordService _wordService = wordService;
    private readonly ICacheService _cacheService = cacheService;
    private static readonly TimeSpan PracticeCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan GetByIdCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<Result> DeleteUnknowsAsync(int unknowsId, string userId)
    {
        var unknow = await _unknowsRepository.GetByIdForUserAsync(unknowsId, userId);
        if (unknow == null)
        {
            return Result.Failure("Bilinmeyen kelime bulunamadı veya size ait değil.");
        }

        _unknowsRepository.Remove(unknow);
        await _unitOfWork.CommitAsync();

        // Cache invalidation
        await _cacheService.RemoveAsync($"unknows:{unknowsId}:user:{userId}");
        await _cacheService.RemoveAsync($"unknows:user:{userId}");

        return Result.Success();
    }

    public async Task<Result<string>> GetRandomWordFromUnknowsAsync(string userId)
    {
        var cacheKey = $"unknows:user:{userId}";
        var cachedUnknows = await _cacheService.GetAsync<List<PracticeUnknowsCacheDto>>(cacheKey);

        List<PracticeUnknowsCacheDto> practiceUnknows;
        if (cachedUnknows != null && cachedUnknows.Count > 0)
        {
            practiceUnknows = cachedUnknows;
        }
        else
        {
            var unknows = await _unknowsRepository.GetUserUnknowsWithWordAsync(userId);
            practiceUnknows = unknows.Select(u => new PracticeUnknowsCacheDto
            {
                EnglishWord = u.Word?.EnglishWord
            }).ToList();

            if (practiceUnknows.Count > 0)
            {
                await _cacheService.SetAsync(cacheKey, practiceUnknows, PracticeCacheExpiration);
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
        return _wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
    }

    public Task<List<Unknows>> GetUserUnknowsAsync(string userId)
    {
        return _unknowsRepository.GetUserUnknowsWithWordAsync(userId);
    }

    public async Task<Result<bool>> ToggleUnknowsAsync(int wordId, string userId)
    {
        var wordExists = await _wordService.GetWordForUserAsync(wordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }

        var existingUnknow = await _unknowsRepository.GetByWordForUserAsync(wordId, userId);
        if (existingUnknow == null)
        {
            var unknow = new Unknows { WordId = wordId, UserId = userId, CreatedTime = DateTime.Now };
            await _unknowsRepository.AddAsync(unknow);
            await _unitOfWork.CommitAsync();

            // Cache invalidation
            await _cacheService.RemoveAsync($"unknows:user:{userId}");

            return Result<bool>.Success(true);
        }

        _unknowsRepository.Remove(existingUnknow);
        await _unitOfWork.CommitAsync();

        // Cache invalidation
        await _cacheService.RemoveAsync($"unknows:user:{userId}");

        return Result<bool>.Success(false);
    }

    public async Task<Result<Unknows>> GetUnknowsWithWordAsync(int unknowsId, string userId)
    {
        var cacheKey = $"unknows:{unknowsId}:user:{userId}";
        var cachedUnknowDto = await _cacheService.GetAsync<UnknowsWithWordDto>(cacheKey);
        if (cachedUnknowDto != null)
        {
            var cachedUnknow = new Unknows
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

        var unknow = await _unknowsRepository.GetUnknowsWithWordAsync(unknowsId, userId);
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
        await _cacheService.SetAsync(cacheKey, unknowDto, GetByIdCacheExpiration);
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

        var (items, totalCount) = await _unknowsRepository.GetPagedUnknowsAsync(userId, search, page, pageSize);
        return Result<(List<Unknows> Unknows, int TotalCount)>.Success((items, totalCount));
    }
}

