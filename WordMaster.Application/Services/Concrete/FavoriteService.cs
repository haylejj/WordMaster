using WordMaster.Application.Dto;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class FavoriteService(IFavoriteRepository favoriteRepository, IUnitOfWork unitOfWork, IWordService wordService, ICacheService cacheService) : IFavoriteService
{
    private static readonly Random _random = new();
    private static readonly TimeSpan PracticeCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan GetByIdCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<Result> DeleteFavoriteAsync(int favoriteId, string userId)
    {
        var favorite = await favoriteRepository.GetByIdForUserAsync(favoriteId, userId);
        if (favorite == null)
        {
            return Result.Failure("Favori bulunamadı veya size ait değil.");
        }

        favoriteRepository.Remove(favorite);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"favorite:{favoriteId}:user:{userId}");
        await cacheService.RemoveAsync($"favorites:user:{userId}");

        return Result.Success();
    }

    public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
    {
        return await favoriteRepository.GetUserFavoritesWithWordAsync(userId);
    }

    public async Task<Result<bool>> ToggleFavoriteAsync(int wordId, string userId)
    {
        var wordExists = await wordService.GetWordForUserAsync(wordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }

        var existingFavorite = await favoriteRepository.GetByWordForUserAsync(wordId, userId);
        if (existingFavorite == null)
        {
            var favorite = new Favorite { WordId = wordId, UserId = userId, CreatedTime = DateTime.Now };
            await favoriteRepository.AddAsync(favorite);
            await unitOfWork.CommitAsync();

            // Cache invalidation
            await cacheService.RemoveAsync($"favorites:user:{userId}");

            return Result<bool>.Success(true);
        }

        favoriteRepository.Remove(existingFavorite);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync($"favorites:user:{userId}");

        return Result<bool>.Success(false);
    }

    public async Task<Result<Favorite>> GetFavoriteWithWordAsync(int favoriteId, string userId)
    {
        var cacheKey = $"favorite:{favoriteId}:user:{userId}";
        var cachedFavoriteDto = await cacheService.GetAsync<FavoriteWithWordDto>(cacheKey);
        if (cachedFavoriteDto != null)
        {
            var cachedFavorite = new Favorite
            {
                Id = cachedFavoriteDto.Id,
                CreatedTime = cachedFavoriteDto.CreatedTime,
                WordId = cachedFavoriteDto.WordId,
                UserId = cachedFavoriteDto.UserId,
                Word = cachedFavoriteDto.Word != null ? new Word
                {
                    Id = cachedFavoriteDto.Word.Id,
                    EnglishWord = cachedFavoriteDto.Word.EnglishWord,
                    TurkishWord = cachedFavoriteDto.Word.TurkishWord
                } : null
            };
            return Result<Favorite>.Success(cachedFavorite);
        }

        var favorite = await favoriteRepository.GetFavoriteWithWordAsync(favoriteId, userId);
        if (favorite == null)
        {
            return Result<Favorite>.Failure("Favori bulunamadı.");
        }

        var favoriteDto = new FavoriteWithWordDto
        {
            Id = favorite.Id,
            CreatedTime = favorite.CreatedTime,
            WordId = favorite.WordId,
            UserId = favorite.UserId,
            Word = favorite.Word != null ? new WordDto
            {
                Id = favorite.Word.Id,
                EnglishWord = favorite.Word.EnglishWord,
                TurkishWord = favorite.Word.TurkishWord
            } : null
        };
        await cacheService.SetAsync(cacheKey, favoriteDto, GetByIdCacheExpiration);
        return Result<Favorite>.Success(favorite);
    }

    public async Task<Result<string>> GetRandomWordFromFavoritesAsync(string userId)
    {
        var cacheKey = $"favorites:user:{userId}";
        var cachedFavorites = await cacheService.GetAsync<List<PracticeFavoriteCacheDto>>(cacheKey);

        List<PracticeFavoriteCacheDto> practiceFavorites;
        if (cachedFavorites != null && cachedFavorites.Count > 0)
        {
            practiceFavorites = cachedFavorites;
        }
        else
        {
            var favorites = await favoriteRepository.GetUserFavoritesWithWordAsync(userId);
            practiceFavorites = [.. favorites.Select(f => new PracticeFavoriteCacheDto
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
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, practiceFavorites.Count);
        return Result<string>.Success(practiceFavorites[index].EnglishWord ?? string.Empty);
    }

    public Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        return wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
    }

    public async Task<Result<(List<Favorite> Favorites, int TotalCount)>> GetPagedFavoritesAsync(string userId, string? search, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        var (favorites, totalCount) = await favoriteRepository.GetPagedFavoritesAsync(userId, search, page, pageSize);
        return Result<(List<Favorite> Favorites, int TotalCount)>.Success((favorites, totalCount));
    }
}

