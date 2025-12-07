using System.Net;
using WordMaster.Application.Constants;
using WordMaster.Application.Key;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Favorite;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Favorite;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class FavoriteService(IFavoriteRepository favoriteRepository, IUnitOfWork unitOfWork, IWordService wordService, ICacheService cacheService) : IFavoriteService
{



    public async Task<ServiceResult> DeleteFavoriteAsync(int favoriteId, Guid userId)
    {
        Favorite? favorite = await favoriteRepository.GetByIdForUserAsync(favoriteId, userId);
        if (favorite == null)
        {
            return ServiceResult.Failure("Favori bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        favoriteRepository.Remove(favorite);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(CacheKeys.Favorite(favoriteId, userId));
        await cacheService.RemoveAsync(CacheKeys.Favorites(userId));

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult<bool>> ToggleFavoriteAsync(ToggleFavoriteRequest request, Guid userId)
    {
        ServiceResult<WordResponse> wordExists = await wordService.GetWordForUserAsync(request.WordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return ServiceResult<bool>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        Favorite? existingFavorite = await favoriteRepository.GetByWordForUserAsync(request.WordId, userId);
        if (existingFavorite == null)
        {
            Favorite favorite = new() { WordId = request.WordId, UserId = userId, CreatedTime = DateTime.UtcNow };
            await favoriteRepository.AddAsync(favorite);
            await unitOfWork.CommitAsync();


            await cacheService.RemoveAsync(CacheKeys.Favorites(userId));

            return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
        }

        favoriteRepository.Remove(existingFavorite);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(CacheKeys.Favorites(userId));

        return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<PracticeWordResponse>> GetRandomWordFromFavoritesAsync(Guid userId)
    {
        string cacheKey = CacheKeys.Favorites(userId);
        List<PracticeFavoriteKey>? cachedFavorites = await cacheService.GetAsync<List<PracticeFavoriteKey>>(cacheKey);

        List<PracticeFavoriteKey> practiceFavorites;
        if (cachedFavorites != null && cachedFavorites.Count > 0)
        {
            practiceFavorites = cachedFavorites;
        }
        else
        {
            List<Favorite> favorites = await favoriteRepository.GetUserFavoritesWithWordAsync(userId);
            practiceFavorites = [.. favorites.Select(f => new PracticeFavoriteKey
            {
                Id = f.Word!.Id,
                EnglishWord = f.Word.EnglishWord,
                TurkishWord = f.Word.TurkishWord
            })];

            if (practiceFavorites.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, practiceFavorites, CacheDurations.Practice);
            }
        }

        if (practiceFavorites.Count == 0)
        {
            return ServiceResult<PracticeWordResponse>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        int index = Random.Shared.Next(0, practiceFavorites.Count);
        PracticeFavoriteKey randomFavorite = practiceFavorites[index];

        PracticeWordResponse response = new()
        {
            Id = randomFavorite.Id,
            EnglishWord = randomFavorite.EnglishWord ?? string.Empty,
            TurkishWord = randomFavorite.TurkishWord ?? string.Empty
        };

        return ServiceResult<PracticeWordResponse>.Success(response, HttpStatusCode.OK);
    }

    public Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request)
    {
        return wordService.CheckTranslationAndUpdateAsync(userId, request);
    }

    public async Task<ServiceResult<PagedResult<FavoriteWithWordResponse>>> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        (List<Favorite>? favorites, int totalCount) = await favoriteRepository.GetPagedFavoritesAsync(userId, search, page, pageSize);

        List<FavoriteWithWordResponse> favoriteDtos = favorites.Select(f => new FavoriteWithWordResponse
        {
            Id = f.Id,
            CreatedTime = f.CreatedTime,
            WordId = f.WordId,
            UserId = f.UserId,
            Word = f.Word != null ? new FavoriteWordResponse
            {
                Id = f.Word.Id,
                EnglishWord = f.Word.EnglishWord ?? string.Empty,
                TurkishWord = f.Word.TurkishWord ?? string.Empty
            } : null
        }).ToList();

        PagedResult<FavoriteWithWordResponse> pagedResult = new()
        {
            Items = favoriteDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<FavoriteWithWordResponse>>.Success(pagedResult, HttpStatusCode.OK);
    }
}
