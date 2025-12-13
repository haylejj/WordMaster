using WordMaster.Application.Requests.Unknows;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Unknows;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUnknowsService
{
    Task<ServiceResult<bool>> ToggleUnknowsAsync(ToggleUnknowsRequest request, Guid userId);
    Task<ServiceResult<PracticeWordResponse>> GetRandomWordFromUnknowsAsync(Guid userId, long? excludeWordId = null);
    Task<ServiceResult<QuizResponse>> GetQuizAsync(Guid userId, long? excludeWordId = null);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request);
    Task<ServiceResult<PagedResult<UnknowsWithWordResponse>>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize);
}
