using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IExcelService
{
    Task<Result> ImportWordsAsync(Stream fileStream, Guid userId);
}
