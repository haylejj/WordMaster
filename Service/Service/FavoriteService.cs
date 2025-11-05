using Core.Entity;
using Core.Extensions;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Service.Service;

public class FavoriteService(IGenericRepository<Favorite> repository, IUnitOfWork unitOfWork) : IFavoriteService
{
    private static readonly Random _random = new();
    public IQueryable<Favorite> Where(Expression<Func<Favorite, bool>> predicate)
    {
        return repository.Where(predicate);
    }
    public async Task AddAsync(Favorite entity)
    {
        await repository.AddAsync(entity);
        await unitOfWork.CommitAsync();
    }
    public async Task RemoveAsync(Favorite entity)
    {
        repository.Remove(entity);
        await unitOfWork.CommitAsync();
    }
    public async Task<(bool Success, string? ErrorMessage)> DeleteFavoriteAsync(int favoriteId, string userId)
    {
        // Favorite'ı kullanıcıya ait mi kontrol et
        var favorite = await Where(x => x.Id == favoriteId && x.UserId == userId).FirstOrDefaultAsync();
        if (favorite == null)
        {
            return (false, "Favori bulunamadı veya size ait değil.");
        }

        await RemoveAsync(favorite);
        return (true, null);
    }
    public async Task<string> GetRandomWordFromFavoritesAsync(string userId)
    {
        var favorites = await Where(x => x.UserId == userId)
            .Include(x => x.Word)
            .ToListAsync();

        if (favorites == null || favorites.Count == 0)
        {
            return string.Empty;
        }

        int index = _random.Next(0, favorites.Count);
        return favorites[index].Word?.EnglishWord ?? string.Empty;
    }

    public async Task<bool> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord, IWordService wordService)
    {
        // Gelen EnglishWord'i normalize et (standart formata getir)
        var normalizedEnglishWord = englishWord?.NormalizeEnglishWord();

        // Karşılaştırma için ToLowerInvariant kullan (kültür sorununu çözer)
        var favorite = await Where(x =>
            x.Word != null &&
            x.UserId == userId &&
            x.Word.EnglishWord != null &&
            x.Word.EnglishWord.ToLowerInvariant() == normalizedEnglishWord.ToLowerInvariant())
            .Include(x => x.Word)
            .FirstOrDefaultAsync();

        if (favorite?.Word == null)
        {
            return false;
        }

        var word = favorite.Word;
        // Karşılaştırma için ToLowerInvariant kullan (kültür sorununu çözer)
        bool isCorrect = word.TurkishWord?.ToLowerInvariant().Trim() == turkishWord?.ToLowerInvariant().Trim();

        // Öğrenme takibini güncelle
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

        await wordService.UpdateAsync(word);

        return isCorrect;
    }
}
