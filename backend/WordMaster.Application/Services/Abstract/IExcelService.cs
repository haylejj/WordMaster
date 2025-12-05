using WordMaster.Application.Responses.Word;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IExcelService
{
    Task<ServiceResult<WordImportSummaryResponse>> ImportWordsAsync(Stream fileStream, Guid userId);
}
