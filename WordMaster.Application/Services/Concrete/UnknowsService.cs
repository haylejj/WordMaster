using System.Net;
using WordMaster.Application.Dto.Practice;
using WordMaster.Application.Dto.Unknows;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Unknows;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class UnknowsService(IUnknowsRepository unknowsRepository, IUnitOfWork unitOfWork, IWordService wordService, ICacheService cacheService) : IUnknowsService
{
    private static readonly Random _random = new();
    private static readonly TimeSpan PracticeCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan GetByIdCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<ServiceResult> DeleteUnknowsAsync(int unknowsId, Guid userId)
    {
        Unknows? unknow = await unknowsRepository.GetByIdForUserAsync(unknowsId, userId);
        if (unknow == null)
        {
            return ServiceResult.Failure("Bilinmeyen kelime bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        unknowsRepository.Remove(unknow);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"unknows:{unknowsId}:user:{userId}");
        await cacheService.RemoveAsync($"unknows:user:{userId}");

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult<string>> GetRandomWordFromUnknowsAsync(Guid userId)
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
            return ServiceResult<string>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        int index = _random.Next(0, practiceUnknows.Count);
        return ServiceResult<string>.Success(practiceUnknows[index].EnglishWord ?? string.Empty, HttpStatusCode.OK);
    }

    public Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request)
    {
        return wordService.CheckTranslationAndUpdateAsync(userId, request);
    }

    public async Task<List<UnknowsWithWordDto>> GetUserUnknowsAsync(Guid userId)
    {
        List<Unknows> unknows = await unknowsRepository.GetUserUnknowsWithWordAsync(userId);
        return unknows.Select(u => new UnknowsWithWordDto
        {
            Id = u.Id,
            CreatedTime = u.CreatedTime,
            WordId = u.WordId,
            UserId = u.UserId,
            Word = u.Word != null ? new WordDto
            {
                Id = u.Word.Id,
                EnglishWord = u.Word.EnglishWord,
                TurkishWord = u.Word.TurkishWord
            } : null
        }).ToList();
    }

    public async Task<ServiceResult<bool>> ToggleUnknowsAsync(ToggleUnknowsRequest request, Guid userId)
    {
        ServiceResult<WordDto> wordExists = await wordService.GetWordForUserAsync(request.WordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return ServiceResult<bool>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        Unknows? existingUnknow = await unknowsRepository.GetByWordForUserAsync(request.WordId, userId);
        if (existingUnknow == null)
        {
            Unknows unknow = new() { WordId = request.WordId, UserId = userId, CreatedTime = DateTime.UtcNow };
            await unknowsRepository.AddAsync(unknow);
            await unitOfWork.CommitAsync();

            // Cache invalidation
            await cacheService.RemoveAsync($"unknows:user:{userId}");

            return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
        }

        unknowsRepository.Remove(existingUnknow);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"unknows:user:{userId}");

        return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UnknowsWithWordDto>> GetUnknowsWithWordAsync(int unknowsId, Guid userId)
    {
        string cacheKey = $"unknows:{unknowsId}:user:{userId}";
        UnknowsWithWordDto? cachedUnknowDto = await cacheService.GetAsync<UnknowsWithWordDto>(cacheKey);
        if (cachedUnknowDto != null)
        {
            return ServiceResult<UnknowsWithWordDto>.Success(cachedUnknowDto, HttpStatusCode.OK);
        }

        Unknows? unknow = await unknowsRepository.GetUnknowsWithWordAsync(unknowsId, userId);
        if (unknow == null)
        {
            return ServiceResult<UnknowsWithWordDto>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        UnknowsWithWordDto unknowDto = new()
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
        return ServiceResult<UnknowsWithWordDto>.Success(unknowDto, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<PagedResult<UnknowsWithWordDto>>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize)
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

        List<UnknowsWithWordDto> itemDtos = items.Select(u => new UnknowsWithWordDto
        {
            Id = u.Id,
            CreatedTime = u.CreatedTime,
            WordId = u.WordId,
            UserId = u.UserId,
            Word = u.Word != null ? new WordDto
            {
                Id = u.Word.Id,
                EnglishWord = u.Word.EnglishWord,
                TurkishWord = u.Word.TurkishWord
            } : null
        }).ToList();

        PagedResult<UnknowsWithWordDto> pagedResult = new()
        {
            Items = itemDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<UnknowsWithWordDto>>.Success(pagedResult, HttpStatusCode.OK);
    }
}
