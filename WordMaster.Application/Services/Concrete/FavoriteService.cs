using System.Net;
using WordMaster.Application.Key;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Favorite;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Favorite;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class FavoriteService(IFavoriteRepository favoriteRepository, IUnitOfWork unitOfWork, IWordService wordService, ICacheService cacheService) : IFavoriteService
{
    private static readonly Random _random = new();
    private static readonly TimeSpan PracticeCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan GetByIdCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<ServiceResult> DeleteFavoriteAsync(int favoriteId, Guid userId)
    {
        Favorite? favorite = await favoriteRepository.GetByIdForUserAsync(favoriteId, userId);
        if (favorite == null)
        {
            return ServiceResult.Failure("Favori bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        favoriteRepository.Remove(favorite);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"favorite:{favoriteId}:user:{userId}");
        await cacheService.RemoveAsync($"favorites:user:{userId}");

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<List<FavoriteWithWordResponse>> GetUserFavoritesAsync(Guid userId)
    {
        List<Favorite> favorites = await favoriteRepository.GetUserFavoritesWithWordAsync(userId);
        return favorites.Select(f => new FavoriteWithWordResponse
        {
            Id = f.Id,
            CreatedTime = f.CreatedTime,
            WordId = f.WordId,
            UserId = f.UserId,
            Word = f.Word != null ? new WordResponse
            {
                Id = f.Word.Id,
                EnglishWord = f.Word.EnglishWord,
                TurkishWord = f.Word.TurkishWord
            } : null
        }).ToList();
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

            // Cache invalidation
            await cacheService.RemoveAsync($"favorites:user:{userId}");

            return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
        }

        favoriteRepository.Remove(existingFavorite);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"favorites:user:{userId}");

        return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<FavoriteWithWordResponse>> GetFavoriteWithWordAsync(int favoriteId, Guid userId)
    {
        string cacheKey = $"favorite:{favoriteId}:user:{userId}";
        FavoriteWithWordResponse? cachedFavoriteDto = await cacheService.GetAsync<FavoriteWithWordResponse>(cacheKey);
        if (cachedFavoriteDto != null)
        {
            return ServiceResult<FavoriteWithWordResponse>.Success(cachedFavoriteDto, HttpStatusCode.OK);
        }

        Favorite? favorite = await favoriteRepository.GetFavoriteWithWordAsync(favoriteId, userId);
        if (favorite == null)
        {
            return ServiceResult<FavoriteWithWordResponse>.Failure("Favori bulunamadı.", HttpStatusCode.NotFound);
        }

        FavoriteWithWordResponse favoriteDto = new()
        {
            Id = favorite.Id,
            CreatedTime = favorite.CreatedTime,
            WordId = favorite.WordId,
            UserId = favorite.UserId,
            Word = favorite.Word != null ? new WordResponse
            {
                Id = favorite.Word.Id,
                EnglishWord = favorite.Word.EnglishWord,
                TurkishWord = favorite.Word.TurkishWord
            } : null
        };
        await cacheService.SetAsync(cacheKey, favoriteDto, GetByIdCacheExpiration);
        return ServiceResult<FavoriteWithWordResponse>.Success(favoriteDto, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<string>> GetRandomWordFromFavoritesAsync(Guid userId)
    {
        string cacheKey = $"favorites:user:{userId}";
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
                EnglishWord = f.Word?.EnglishWord
            })];

            if (practiceFavorites.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, practiceFavorites, PracticeCacheExpiration);
            }
        }

        if (practiceFavorites.Count == 0)
        {
            return ServiceResult<string>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        int index = _random.Next(0, practiceFavorites.Count);
        return ServiceResult<string>.Success(practiceFavorites[index].EnglishWord ?? string.Empty, HttpStatusCode.OK);
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
            Word = f.Word != null ? new WordResponse
            {
                Id = f.Word.Id,
                EnglishWord = f.Word.EnglishWord,
                TurkishWord = f.Word.TurkishWord
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
