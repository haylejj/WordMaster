using WordMaster.Application.Dto.Word;
using WordMaster.Application.Requests.Word;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IWordService
{
    Task<ServiceResult<WordDto>> GetWordForUserAsync(long id, Guid userId);
    Task<ServiceResult> AddWordAsync(CreateWordRequest request, Guid userId);
    Task<ServiceResult> UpdateWordAsync(long wordId, UpdateWordRequest request, Guid userId);
    Task<ServiceResult> DeleteWordAsync(long wordId, Guid userId);
    Task<ServiceResult<bool>> IsWordDuplicateAsync(string englishWord, Guid userId);
    Task<ServiceResult<string>> GetRandomWordAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<ServiceResult<PagedResult<WordDto>>> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize);
    Task<ServiceResult<List<WordLookupDto>>> GetUserWordsAsync(Guid userId);
}
