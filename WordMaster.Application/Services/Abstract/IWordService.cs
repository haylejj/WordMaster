using WordMaster.Application.Dto.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IWordService
{
    Task<ServiceResult<Word>> GetWordForUserAsync(long id, Guid userId);
    Task<ServiceResult> AddWordAsync(WordDto wordDto, Guid userId);
    Task<ServiceResult> UpdateWordAsync(long wordId, WordDto wordDto, Guid userId);
    Task<ServiceResult> DeleteWordAsync(long wordId, Guid userId);
    Task<ServiceResult<bool>> IsWordDuplicateAsync(string englishWord, Guid userId);
    Task<ServiceResult<string>> GetRandomWordAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<ServiceResult<(List<Word> Words, int TotalCount)>> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize);
}
