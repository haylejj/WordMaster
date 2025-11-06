using Core.Entity;
using Core.Extensions;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Core.Results;

namespace Service.Service;

public class FavoriteService(IGenericRepository<Favorite> repository, IUnitOfWork unitOfWork, IWordService wordService) : IFavoriteService
{
    private static readonly Random _random = new();
    private readonly IWordService _wordService = wordService;
    public IQueryable<Favorite> Where(Expression<Func<Favorite, bool>> predicate) => repository.Where(predicate);

    public async Task<Result> DeleteFavoriteAsync(int favoriteId, string userId)
    {
        // Favorite'ı kullanıcıya ait mi kontrol et
        var favorite = await Where(x => x.Id == favoriteId && x.UserId == userId).FirstOrDefaultAsync();
        if (favorite == null)
        {
            return Result.Failure("Favori bulunamadı veya size ait değil.");
        }

        repository.Remove(favorite);
        await unitOfWork.CommitAsync();
        return Result.Success();
    }
    public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
    {
        return await Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
    }
    public async Task<Result<bool>> ToggleFavoriteAsync(int wordId, string userId)
    {
        var wordExists = await _wordService.GetWordForUserAsync(wordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }
        var existingFavorite = await Where(x => x.WordId == wordId && x.UserId == userId).FirstOrDefaultAsync();
        if (existingFavorite == null)
        {
            var favorite = new Favorite { WordId = wordId, UserId = userId, CreatedTime = DateTime.Now };
            await repository.AddAsync(favorite);
            await unitOfWork.CommitAsync();
            return Result<bool>.Success(true);
        }
        repository.Remove(existingFavorite);
        await unitOfWork.CommitAsync();
        return Result<bool>.Success(false);
    }
    public async Task<Result<Favorite>> GetFavoriteWithWordAsync(int favoriteId, string userId)
    {
        var favorite = await Where(x => x.Id == favoriteId && x.UserId == userId).Include(x => x.Word).FirstOrDefaultAsync();
        return favorite == null ? Result<Favorite>.Failure("Favori bulunamadı.") : Result<Favorite>.Success(favorite);
    }
    public async Task<Result<string>> GetRandomWordFromFavoritesAsync(string userId)
    {
        var favorites = await Where(x => x.UserId == userId)
            .Include(x => x.Word)
            .ToListAsync();

        if (favorites == null || favorites.Count == 0)
        {
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, favorites.Count);
        return Result<string>.Success(favorites[index].Word?.EnglishWord ?? string.Empty);
    }

    public async Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        return await _wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
    }

    public async Task<Result<(List<Favorite> Favorites, int TotalCount)>> GetPagedFavoritesAsync(string userId, string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = Where(x => x.UserId == userId)
            .Include(x => x.Word)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Word!.EnglishWord!.Contains(search) || x.Word!.TurkishWord!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var favorites = await query
            .OrderBy(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<(List<Favorite> Favorites, int TotalCount)>.Success((favorites, totalCount));
    }
}
