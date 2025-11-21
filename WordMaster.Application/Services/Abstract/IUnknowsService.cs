using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUnknowsService
{
    Task<List<Unknows>> GetUserUnknowsAsync(Guid userId);
    Task<ServiceResult<bool>> ToggleUnknowsAsync(long wordId, Guid userId);
    Task<ServiceResult<Unknows>> GetUnknowsWithWordAsync(int unknowsId, Guid userId);
    Task<ServiceResult> DeleteUnknowsAsync(int unknowsId, Guid userId);
    Task<ServiceResult<string>> GetRandomWordFromUnknowsAsync(Guid userId);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<ServiceResult<(List<Unknows> Unknows, int TotalCount)>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize);
}
