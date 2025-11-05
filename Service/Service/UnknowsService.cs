using Core.Entity;
using Core.Extensions;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Service.Service;

public class UnknowsService(IGenericRepository<Unknows> repository, IUnitOfWork unitOfWork) : IUnknowsService
{
    private static readonly Random _random = new();
    public IQueryable<Unknows> Where(Expression<Func<Unknows, bool>> predicate)
    {
        return repository.Where(predicate);
    }
    public async Task AddAsync(Unknows entity)
    {
        await repository.AddAsync(entity);
        await unitOfWork.CommitAsync();
    }
    public async Task RemoveAsync(Unknows entity)
    {
        repository.Remove(entity);
        await unitOfWork.CommitAsync();
    }
    public async Task<(bool Success, string? ErrorMessage)> DeleteUnknowsAsync(int unknowsId, string userId)
    {
        // Unknows'ı kullanıcıya ait mi kontrol et
        var unknow = await Where(x => x.Id == unknowsId && x.UserId == userId).FirstOrDefaultAsync();
        if (unknow == null)
        {
            return (false, "Bilinmeyen kelime bulunamadı veya size ait değil.");
        }

        await RemoveAsync(unknow);
        return (true, null);
    }

    public async Task<string> GetRandomWordFromUnknowsAsync(string userId)
    {
        var unknows = await Where(x => x.UserId == userId)
            .Include(x => x.Word)
            .ToListAsync();
        
        if (unknows == null || unknows.Count == 0)
        {
            return string.Empty;
        }

        int index = _random.Next(0, unknows.Count);
        return unknows[index].Word?.EnglishWord ?? string.Empty;
    }

    public async Task<bool> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord, IWordService wordService)
    {
        // Gelen EnglishWord'i normalize et (standart formata getir)
        var normalizedEnglishWord = englishWord?.NormalizeEnglishWord();
        
        // Karşılaştırma için ToLowerInvariant kullan (kültür sorununu çözer)
        var unknow = await Where(x => 
            x.Word != null && 
            x.UserId == userId &&
            x.Word.EnglishWord != null &&
            x.Word.EnglishWord.ToLowerInvariant() == normalizedEnglishWord.ToLowerInvariant())
            .Include(x => x.Word)
            .FirstOrDefaultAsync();

        if (unknow?.Word == null)
        {
            return false;
        }

        var word = unknow.Word;
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
