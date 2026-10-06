namespace DotnetApiStarter.Api.Common.Responses;

public class ResponseResult<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public IDictionary<string, string[]>? Errors { get; init; }

    public static ResponseResult<T> Ok(
        T data,
        string message = "Request successful.")
    {
        return new ResponseResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ResponseResult<T> Fail(string message, IDictionary<string, string[]>? errors = null)
    {
        return new ResponseResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
