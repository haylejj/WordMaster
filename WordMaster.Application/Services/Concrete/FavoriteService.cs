using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class FavoriteService(IFavoriteRepository favoriteRepository, IUnitOfWork unitOfWork, IWordService wordService) : IFavoriteService
{
    private static readonly Random _random = new();
    private readonly IFavoriteRepository _favoriteRepository = favoriteRepository;
    private readonly IWordService _wordService = wordService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> DeleteFavoriteAsync(int favoriteId, string userId)
    {
        var favorite = await _favoriteRepository.GetByIdForUserAsync(favoriteId, userId);
        if (favorite == null)
        {
            return Result.Failure("Favori bulunamadı veya size ait değil.");
        }

        _favoriteRepository.Remove(favorite);
        await _unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
    {
        return await _favoriteRepository.GetUserFavoritesWithWordAsync(userId);
    }

    public async Task<Result<bool>> ToggleFavoriteAsync(int wordId, string userId)
    {
        var wordExists = await _wordService.GetWordForUserAsync(wordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }

        var existingFavorite = await _favoriteRepository.GetByWordForUserAsync(wordId, userId);
        if (existingFavorite == null)
        {
            var favorite = new Favorite { WordId = wordId, UserId = userId, CreatedTime = DateTime.Now };
            await _favoriteRepository.AddAsync(favorite);
            await _unitOfWork.CommitAsync();
            return Result<bool>.Success(true);
        }

        _favoriteRepository.Remove(existingFavorite);
        await _unitOfWork.CommitAsync();
        return Result<bool>.Success(false);
    }

    public async Task<Result<Favorite>> GetFavoriteWithWordAsync(int favoriteId, string userId)
    {
        var favorite = await _favoriteRepository.GetFavoriteWithWordAsync(favoriteId, userId);
        return favorite == null
            ? Result<Favorite>.Failure("Favori bulunamadı.")
            : Result<Favorite>.Success(favorite);
    }

    public async Task<Result<string>> GetRandomWordFromFavoritesAsync(string userId)
    {
        var favorites = await _favoriteRepository.GetUserFavoritesWithWordAsync(userId);
        if (favorites.Count == 0)
        {
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, favorites.Count);
        return Result<string>.Success(favorites[index].Word?.EnglishWord ?? string.Empty);
    }

    public Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        return _wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
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

        var (favorites, totalCount) = await _favoriteRepository.GetPagedFavoritesAsync(userId, search, page, pageSize);
        return Result<(List<Favorite> Favorites, int TotalCount)>.Success((favorites, totalCount));
    }
}

