using WordMaster.Application.Dto.Unknows;
using WordMaster.Application.Requests.Unknows;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUnknowsService
{
    Task<List<UnknowsWithWordDto>> GetUserUnknowsAsync(Guid userId);
    Task<ServiceResult<bool>> ToggleUnknowsAsync(ToggleUnknowsRequest request, Guid userId);
    Task<ServiceResult<UnknowsWithWordDto>> GetUnknowsWithWordAsync(int unknowsId, Guid userId);
    Task<ServiceResult> DeleteUnknowsAsync(int unknowsId, Guid userId);
    Task<ServiceResult<string>> GetRandomWordFromUnknowsAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<ServiceResult<PagedResult<UnknowsWithWordDto>>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize);
}
