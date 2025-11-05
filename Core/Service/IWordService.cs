using Core.Dto;
using Core.Entity;
using System.Linq.Expressions;

namespace Core.Service;

public interface IWordService
{
    IQueryable<Word> Where(Expression<Func<Word, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<Word, bool>> predicate);
    Task AddAsync(Word entity);
    Task UpdateAsync(Word entity);
    Task RemoveAsync(Word entity);
    Task<(bool Success, string? ErrorMessage)> AddWordAsync(WordDto wordDto, string userId);
    Task<(bool Success, string? ErrorMessage)> UpdateWordAsync(int wordId, WordDto wordDto, string userId);
    Task<(bool Success, string? ErrorMessage)> DeleteWordAsync(int wordId, string userId);
    Task<bool> IsWordDuplicateAsync(string englishWord, string userId);
    Task<string> GetRandomWordAsync(string userId);
    Task<bool> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord);
}
