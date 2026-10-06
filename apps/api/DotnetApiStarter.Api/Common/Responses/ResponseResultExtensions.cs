namespace DotnetApiStarter.Api.Common.Responses;

public static class ResponseResultExtensions
{
    public static IResult Ok<T>(this ResponseResult<T> response)
    {
        return Results.Ok(response);
    }

    public static IResult Created<T>(this ResponseResult<T> response, string location)
    {
        return Results.Created(location, response);
    }

    public static IResult NoContent<T>(this ResponseResult<T> response)
    {
        return Results.NoContent();
    }
}
