using WordMaster.Application.Requests.Unknows;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Unknows;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUnknowsService
{
    Task<List<UnknowsWithWordResponse>> GetUserUnknowsAsync(Guid userId);
    Task<ServiceResult<bool>> ToggleUnknowsAsync(ToggleUnknowsRequest request, Guid userId);
    Task<ServiceResult<UnknowsWithWordResponse>> GetUnknowsWithWordAsync(int unknowsId, Guid userId);
    Task<ServiceResult> DeleteUnknowsAsync(int unknowsId, Guid userId);
    Task<ServiceResult<string>> GetRandomWordFromUnknowsAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request);
    Task<ServiceResult<PagedResult<UnknowsWithWordResponse>>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize);
}
