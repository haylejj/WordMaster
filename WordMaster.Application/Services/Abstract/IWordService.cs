using WordMaster.Application.Dto.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IWordService
{
    Task<Result<Word>> GetWordForUserAsync(long id, Guid userId);
    Task<Result> AddWordAsync(WordDto wordDto, Guid userId);
    Task<Result> UpdateWordAsync(long wordId, WordDto wordDto, Guid userId);
    Task<Result> DeleteWordAsync(long wordId, Guid userId);
    Task<Result<bool>> IsWordDuplicateAsync(string englishWord, Guid userId);
    Task<Result<string>> GetRandomWordAsync(Guid userId);
    Task<Result<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<Result<(List<Word> Words, int TotalCount)>> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize);
}
