using WordMaster.Domain.Results;

namespace WordMaster.WebUI.Extensions;

public static class ServiceResultExtensions
{
    public static string? ErrorMessage(this ServiceResult result)
    {
        return result.ErrorList?.FirstOrDefault();
    }

    public static string? ErrorMessage<T>(this ServiceResult<T> result)
    {
        return result.ErrorList?.FirstOrDefault();
    }
}

