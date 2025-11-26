using System.Net;
using WordMaster.Application.Key;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Unknows;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Unknows;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

using WordMaster.Application.Constants;

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
        await cacheService.RemoveAsync(CacheKeys.Unknow(unknowsId, userId));
        await cacheService.RemoveAsync(CacheKeys.Unknows(userId));

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult<PracticeWordResponse>> GetRandomWordFromUnknowsAsync(Guid userId)
    {
        string cacheKey = CacheKeys.Unknows(userId);
        List<PracticeUnknowsKey>? cachedUnknows = await cacheService.GetAsync<List<PracticeUnknowsKey>>(cacheKey);

        List<PracticeUnknowsKey> practiceUnknows;
        if (cachedUnknows != null && cachedUnknows.Count > 0)
        {
            practiceUnknows = cachedUnknows;
        }
        else
        {
            List<Unknows> unknows = await unknowsRepository.GetUserUnknowsWithWordAsync(userId);
            practiceUnknows = unknows.Select(u => new PracticeUnknowsKey
            {
                Id = u.Word!.Id,
                EnglishWord = u.Word.EnglishWord,
                TurkishWord = u.Word.TurkishWord
            }).ToList();

            if (practiceUnknows.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, practiceUnknows, PracticeCacheExpiration);
            }
        }

        if (practiceUnknows.Count == 0)
        {
            return ServiceResult<PracticeWordResponse>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        int index = _random.Next(0, practiceUnknows.Count);
        PracticeUnknowsKey randomUnknow = practiceUnknows[index];

        PracticeWordResponse response = new()
        {
            Id = randomUnknow.Id,
            EnglishWord = randomUnknow.EnglishWord ?? string.Empty,
            TurkishWord = randomUnknow.TurkishWord ?? string.Empty
        };

        return ServiceResult<PracticeWordResponse>.Success(response, HttpStatusCode.OK);
    }

    public Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request)
    {
        return wordService.CheckTranslationAndUpdateAsync(userId, request);
    }
    public async Task<ServiceResult<bool>> ToggleUnknowsAsync(ToggleUnknowsRequest request, Guid userId)
    {
        ServiceResult<WordResponse> wordExists = await wordService.GetWordForUserAsync(request.WordId, userId);
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
            await cacheService.RemoveAsync(CacheKeys.Unknows(userId));

            return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
        }

        unknowsRepository.Remove(existingUnknow);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync(CacheKeys.Unknows(userId));

        return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<PagedResult<UnknowsWithWordResponse>>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize)
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

        List<UnknowsWithWordResponse> itemDtos = items.Select(u => new UnknowsWithWordResponse
        {
            Id = u.Id,
            CreatedTime = u.CreatedTime,
            WordId = u.WordId,
            UserId = u.UserId,
            Word = u.Word != null ? new WordResponse
            {
                Id = u.Word.Id,
                EnglishWord = u.Word.EnglishWord,
                TurkishWord = u.Word.TurkishWord
            } : null
        }).ToList();

        PagedResult<UnknowsWithWordResponse> pagedResult = new()
        {
            Items = itemDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<UnknowsWithWordResponse>>.Success(pagedResult, HttpStatusCode.OK);
    }
}
