using System.Net;
using System.Text.Json.Serialization;

namespace WordMaster.Domain.Results;

public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public List<string>? ErrorList { get; set; }
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }
    public string? UrlAsCreated { get; set; }

    public static ServiceResult Success(HttpStatusCode statusCode)
    {
        return new()
        {
            StatusCode = statusCode,
            IsSuccess = true
        };
    }
    public static ServiceResult SuccessAsCreated(string? url = null)
    {
        return new()
        {
            StatusCode = HttpStatusCode.Created,
            UrlAsCreated = url,
            IsSuccess = true
        };
    }
    public static ServiceResult Failure(List<string> errorList, HttpStatusCode statusCode)
    {
        return new()
        {
            ErrorList = errorList,
            StatusCode = statusCode,
            IsSuccess = false
        };
    }

    public static ServiceResult Failure(string errorMessage, HttpStatusCode statusCode)
    {
        return new()
        {
            ErrorList = [errorMessage],
            StatusCode = statusCode,
            IsSuccess = false
        };
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public List<string>? ErrorList { get; set; }
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }
    public string? UrlAsCreated { get; set; }

    public static ServiceResult<T> Success(T data, HttpStatusCode statusCode)
    {
        return new()
        {
            Data = data,
            StatusCode = statusCode,
            IsSuccess = true
        };
    }
    public static ServiceResult<T> SuccessAsCreated(T data, string? url = null)
    {
        return new()
        {
            Data = data,
            StatusCode = HttpStatusCode.Created,
            UrlAsCreated = url,
            IsSuccess = true
        };
    }
    public new static ServiceResult<T> Failure(List<string> errorList, HttpStatusCode statusCode)
    {
        return new()
        {
            ErrorList = errorList,
            StatusCode = statusCode,
            IsSuccess = false
        };
    }

    public new static ServiceResult<T> Failure(string errorMessage, HttpStatusCode statusCode)
    {
        return new()
        {
            ErrorList = [errorMessage],
            StatusCode = statusCode,
            IsSuccess = false
        };
    }
}


