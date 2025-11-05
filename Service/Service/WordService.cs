using Core.Dto;
using Core.Entity;
using Core.Extensions;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Service.Service;

public class WordService(IGenericRepository<Word> repository, IUnitOfWork unitOfWork, IWordRepository wordRepository) : IWordService
{
    private static readonly Random _random = new();
    public IQueryable<Word> Where(Expression<Func<Word, bool>> predicate)
    {
        return repository.Where(predicate);
    }
    public async Task<bool> AnyAsync(Expression<Func<Word, bool>> predicate)
    {
        return await repository.AnyAsync(predicate);
    }
    public async Task AddAsync(Word entity)
    {
        await repository.AddAsync(entity);
        await unitOfWork.CommitAsync();
    }
    public async Task UpdateAsync(Word entity)
    {
        repository.Update(entity);
        await unitOfWork.CommitAsync();
    }
    public async Task RemoveAsync(Word entity)
    {
        repository.Remove(entity);
        await unitOfWork.CommitAsync();
    }
    public async Task<(bool Success, string? ErrorMessage)> AddWordAsync(WordDto wordDto, string userId)
    {
        // Normalize kelimeleri - standart formata getir
        wordDto.EnglishWord = wordDto.EnglishWord?.NormalizeEnglishWord();
        wordDto.TurkishWord = wordDto.TurkishWord?.NormalizeTurkishWord();

        // EnglishWord boş kontrolü
        if (string.IsNullOrWhiteSpace(wordDto.EnglishWord))
        {
            return (false, "İngilizce kelime boş olamaz.");
        }

        // Duplicate kontrol
        var isDuplicate = await IsWordDuplicateAsync(wordDto.EnglishWord, userId);
        if (isDuplicate)
        {
            return (false, "Bu kelime zaten sözlüğünüzde mevcut.");
        }

        // Entity oluştur ve kaydet
        var word = new Word
        {
            EnglishWord = wordDto.EnglishWord,
            TurkishWord = wordDto.TurkishWord,
            UserId = userId,
            CreatedTime = DateTime.Now
        };

        await AddAsync(word);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateWordAsync(int wordId, WordDto wordDto, string userId)
    {
        // Kelimeyi kullanıcıya ait mi kontrol et
        var existingWord = await Where(x => x.Id == wordId && x.UserId == userId).FirstOrDefaultAsync();
        if (existingWord == null)
        {
            return (false, "Kelime bulunamadı veya size ait değil.");
        }

        // Normalize kelimeleri - standart formata getir
        wordDto.EnglishWord = wordDto.EnglishWord?.NormalizeEnglishWord();
        wordDto.TurkishWord = wordDto.TurkishWord?.NormalizeTurkishWord();

        // EnglishWord boş kontrolü
        if (string.IsNullOrWhiteSpace(wordDto.EnglishWord))
        {
            return (false, "İngilizce kelime boş olamaz.");
        }

        // Duplicate kontrol - kendi kelimesi hariç
        var isDuplicate = await AnyAsync(x =>
            x.Id != wordId &&
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord.ToLowerInvariant() == wordDto.EnglishWord.ToLowerInvariant());

        if (isDuplicate)
        {
            return (false, "Bu kelime zaten sözlüğünüzde mevcut.");
        }

        // Güncelle
        existingWord.EnglishWord = wordDto.EnglishWord;
        existingWord.TurkishWord = wordDto.TurkishWord;

        await UpdateAsync(existingWord);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteWordAsync(int wordId, string userId)
    {
        // Kelimeyi kullanıcıya ait mi kontrol et
        var word = await Where(x => x.Id == wordId && x.UserId == userId).FirstOrDefaultAsync();
        if (word == null)
        {
            return (false, "Kelime bulunamadı veya size ait değil.");
        }

        await RemoveAsync(word);
        return (true, null);
    }

    public async Task<bool> IsWordDuplicateAsync(string englishWord, string userId)
    {
        if (string.IsNullOrWhiteSpace(englishWord))
            return false;

        var normalizedWord = englishWord.NormalizeEnglishWord();
        var exists = await AnyAsync(x =>
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord.ToLowerInvariant() == normalizedWord.ToLowerInvariant());

        return exists;
    }

    public async Task<string> GetRandomWordAsync(string userId)
    {
        var words = await Where(x => x.UserId == userId).ToListAsync();
        if (words == null || words.Count == 0)
        {
            return string.Empty;
        }

        int index = _random.Next(0, words.Count);
        return words[index].EnglishWord ?? string.Empty;
    }

    public async Task<bool> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        // Gelen EnglishWord'i normalize et (standart formata getir)
        var normalizedEnglishWord = englishWord?.NormalizeEnglishWord();

        // Karşılaştırma için ToLowerInvariant kullan (kültür sorununu çözer)
        var word = await Where(x =>
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord.ToLowerInvariant() == normalizedEnglishWord.ToLowerInvariant())
            .FirstOrDefaultAsync();

        if (word == null)
        {
            return false;
        }

        // Karşılaştırma için ToLowerInvariant kullan (kültür sorununu çözer)
        bool isCorrect = word.TurkishWord?.ToLowerInvariant().Trim() == turkishWord?.ToLowerInvariant().Trim();

        // Öğrenme takibini güncelle
        word.IsLastAnswerCorrect = isCorrect;
        word.LastPracticeDate = DateTime.Now;

        if (isCorrect)
        {
            word.ConsecutiveCorrectCount++;
            word.ConsecutiveWrongCount = 0; // Ard arda doğru bildiyse yanlış sayacını sıfırla
            word.TotalCorrectCount++;
        }
        else
        {
            word.ConsecutiveWrongCount++;
            word.ConsecutiveCorrectCount = 0; // Yanlış bildiyse doğru sayacını sıfırla
            word.TotalWrongCount++;
        }

        await UpdateAsync(word);

        return isCorrect;
    }
}
