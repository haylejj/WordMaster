using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IUnknowsService
{
    Task<List<Unknows>> GetUserUnknowsAsync(string userId);
    Task<Result<bool>> ToggleUnknowsAsync(int wordId, string userId);
    Task<Result<Unknows>> GetUnknowsWithWordAsync(int unknowsId, string userId);
    Task<Result> DeleteUnknowsAsync(int unknowsId, string userId);
    Task<Result<string>> GetRandomWordFromUnknowsAsync(string userId);
    Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord);
    Task<Result<(List<Unknows> Unknows, int TotalCount)>> GetPagedUnknowsAsync(string userId, string? search, int page, int pageSize);
}
