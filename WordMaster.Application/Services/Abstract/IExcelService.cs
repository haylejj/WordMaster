using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IExcelService
{
    Task<ServiceResult> ImportWordsAsync(Stream fileStream, Guid userId);
}
