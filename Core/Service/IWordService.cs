using Core.Dto;
using Core.Entity;
using System.Linq.Expressions;
using Core.Results;

namespace Core.Service;

public interface IWordService
{
    IQueryable<Word> Where(Expression<Func<Word, bool>> predicate);
    Task<Result<Word>> GetWordForUserAsync(int id, string userId);
    Task<Result> AddWordAsync(WordDto wordDto, string userId);
    Task<Result> UpdateWordAsync(int wordId, WordDto wordDto, string userId);
    Task<Result> DeleteWordAsync(int wordId, string userId);
    Task<Result<bool>> IsWordDuplicateAsync(string englishWord, string userId);
    Task<Result<string>> GetRandomWordAsync(string userId);
    Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord);
    Task<Result<(List<Word> Words, int TotalCount)>> GetPagedWordsAsync(string userId, string? search, int page, int pageSize);
}
