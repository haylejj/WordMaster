using Core.Dto;
using Core.Entity;
using Core.Extensions;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Core.Results;

namespace Service.Service;

public class WordService(IGenericRepository<Word> repository, IUnitOfWork unitOfWork) : IWordService
{
    private static readonly Random _random = new();
    public IQueryable<Word> Where(Expression<Func<Word, bool>> predicate) => repository.Where(predicate);

    public async Task<Result<Word>> GetWordForUserAsync(int id, string userId)
    {
        var word = await Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        return word == null ? Result<Word>.Failure("Kelime bulunamadı.") : Result<Word>.Success(word);
    }
    public async Task<Result> AddWordAsync(WordDto wordDto, string userId)
    {
        // Normalize kelimeleri - standart formata getir
        wordDto.EnglishWord = wordDto.EnglishWord?.NormalizeEnglishWord();
        wordDto.TurkishWord = wordDto.TurkishWord?.NormalizeTurkishWord();

        // EnglishWord boş kontrolü
        if (string.IsNullOrWhiteSpace(wordDto.EnglishWord))
        {
            return Result.Failure("İngilizce kelime boş olamaz.");
        }

        // Duplicate kontrol
        var isDuplicate = await IsWordDuplicateAsync(wordDto.EnglishWord, userId);
        if (isDuplicate.Data == true)
        {
            return Result.Failure("Bu kelime zaten sözlüğünüzde mevcut.");
        }

        // Entity oluştur ve kaydet
        var word = new Word
        {
            EnglishWord = wordDto.EnglishWord,
            TurkishWord = wordDto.TurkishWord,
            UserId = userId,
            CreatedTime = DateTime.Now
        };

        await repository.AddAsync(word);
        await unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateWordAsync(int wordId, WordDto wordDto, string userId)
    {
        // Kelimeyi kullanıcıya ait mi kontrol et
        var existingWord = await Where(x => x.Id == wordId && x.UserId == userId).FirstOrDefaultAsync();
        if (existingWord == null)
        {
            return Result.Failure("Kelime bulunamadı veya size ait değil.");
        }

        // Normalize kelimeleri - standart formata getir
        wordDto.EnglishWord = wordDto.EnglishWord?.NormalizeEnglishWord();
        wordDto.TurkishWord = wordDto.TurkishWord?.NormalizeTurkishWord();

        // EnglishWord boş kontrolü
        if (string.IsNullOrWhiteSpace(wordDto.EnglishWord))
        {
            return Result.Failure("İngilizce kelime boş olamaz.");
        }

        // Duplicate kontrol - kendi kelimesi hariç
        var isDuplicate = await repository.AnyAsync(x =>
            x.Id != wordId &&
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord == wordDto.EnglishWord);

        if (isDuplicate)
        {
            return Result.Failure("Bu kelime zaten sözlüğünüzde mevcut.");
        }

        // Güncelle
        existingWord.EnglishWord = wordDto.EnglishWord;
        existingWord.TurkishWord = wordDto.TurkishWord;

        repository.Update(existingWord);
        await unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteWordAsync(int wordId, string userId)
    {
        // Kelimeyi kullanıcıya ait mi kontrol et
        var word = await Where(x => x.Id == wordId && x.UserId == userId).FirstOrDefaultAsync();
        if (word == null)
        {
            return Result.Failure("Kelime bulunamadı veya size ait değil.");
        }

        repository.Remove(word);
        await unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<bool>> IsWordDuplicateAsync(string englishWord, string userId)
    {
        if (string.IsNullOrWhiteSpace(englishWord))
            return Result<bool>.Success(false);

        var normalizedWord = englishWord.NormalizeEnglishWord();
        var exists = await repository.AnyAsync(x =>
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord == normalizedWord);

        return Result<bool>.Success(exists);
    }

    public async Task<Result<string>> GetRandomWordAsync(string userId)
    {
        var words = await Where(x => x.UserId == userId).ToListAsync();
        if (words == null || words.Count == 0)
        {
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, words.Count);
        return Result<string>.Success(words[index].EnglishWord ?? string.Empty);
    }

    public async Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        var normalizedEnglishWord = englishWord?.NormalizeEnglishWord();

        var word = await Where(x =>
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord == normalizedEnglishWord)
            .FirstOrDefaultAsync();

        if (word == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }

        var normalizedTurkishWord = turkishWord?.NormalizeTurkishWord();
        bool isCorrect = word.TurkishWord == normalizedTurkishWord;

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

        repository.Update(word);
        await unitOfWork.CommitAsync();

        return Result<bool>.Success(isCorrect);
    }

    public async Task<Result<(List<Word> Words, int TotalCount)>> GetPagedWordsAsync(string userId, string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = Where(x => x.UserId == userId)
            .Include(x => x.Favorite)
            .Include(x => x.Unknows)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.EnglishWord!.Contains(search) || x.TurkishWord!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var words = await query
            .OrderBy(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<(List<Word> Words, int TotalCount)>.Success((words, totalCount));
    }
}
