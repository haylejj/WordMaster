using WordMaster.Application.Requests.Practice;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Word;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IWordService
{
    Task<ServiceResult<WordResponse>> GetWordForUserAsync(long id, Guid userId);
    Task<ServiceResult> AddWordAsync(CreateWordRequest request, Guid userId);
    Task<ServiceResult<WordResponse>> UpdateWordAsync(long wordId, UpdateWordRequest request, Guid userId);
    Task<ServiceResult> DeleteWordAsync(long wordId, Guid userId);
    Task<ServiceResult<bool>> IsWordDuplicateAsync(string englishWord, Guid userId);
    Task<ServiceResult<PracticeWordResponse>> GetRandomWordAsync(Guid userId, long? excludeWordId = null);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request);
    Task<ServiceResult<bool>> BulkUpdateStatsAsync(Guid userId, BulkUpdateStatsRequest request);
    Task<ServiceResult<PagedResult<WordResponse>>> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize);
    Task<ServiceResult<List<WordLookupResponse>>> GetUserWordsAsync(Guid userId);

    // Admin Methods
    Task<ServiceResult<PagedResult<AdminWordResponse>>> GetAdminPagedWordsAsync(string? search, int page, int pageSize);
    Task<ServiceResult<AdminWordResponse>> AdminUpdateWordAsync(long wordId, UpdateWordRequest request);
    Task<ServiceResult> AdminDeleteWordAsync(long wordId);
}
