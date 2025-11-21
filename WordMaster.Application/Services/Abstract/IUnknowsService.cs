using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUnknowsService
{
    Task<List<Unknows>> GetUserUnknowsAsync(Guid userId);
    Task<Result<bool>> ToggleUnknowsAsync(long wordId, Guid userId);
    Task<Result<Unknows>> GetUnknowsWithWordAsync(int unknowsId, Guid userId);
    Task<Result> DeleteUnknowsAsync(int unknowsId, Guid userId);
    Task<Result<string>> GetRandomWordFromUnknowsAsync(Guid userId);
    Task<Result<bool>> CheckTranslationAndUpdateAsync(Guid userId, string turkishWord, string englishWord);
    Task<Result<(List<Unknows> Unknows, int TotalCount)>> GetPagedUnknowsAsync(Guid userId, string? search, int page, int pageSize);
}
