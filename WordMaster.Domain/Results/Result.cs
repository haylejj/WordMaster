namespace WordMaster.Domain.Results;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result Success()
    {
        return new() { IsSuccess = true };
    }

    public static Result Failure(string? errorMessage)
    {
        return new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Success(T? data)
    {
        return new() { IsSuccess = true, Data = data };
    }

    public new static Result<T> Failure(string? errorMessage)
    {
        return new() { IsSuccess = false, ErrorMessage = errorMessage, Data = default };
    }
}


