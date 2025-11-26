using WordMaster.Application.Requests.Unknows;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Unknows;
using WordMaster.Application.Responses.Word;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUnknowsService
{
    Task<ServiceResult<bool>> ToggleUnknowsAsync(ToggleUnknowsRequest request, Guid userId);
    Task<ServiceResult> DeleteUnknowsAsync(int unknowsId, Guid userId);
    Task<ServiceResult<PracticeWordResponse>> GetRandomWordFromUnknowsAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request);
    Task<ServiceResult<PagedResult<UnknowsWithWordResponse>>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize);
}
