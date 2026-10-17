using DotnetApiStarter.Api.Common.Responses;
using DotnetApiStarter.Application.Features.Authentication.Commands.Login;
using DotnetApiStarter.Application.Features.Authentication.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApiStarter.Api.Features.Authentication;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication").AllowAnonymous();

        group.MapPost("/login", LoginAsync);
        group.MapPost("/register", RegisterAsync);

        return app;
    }

    private static async Task<IResult> LoginAsync([FromBody] LoginCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        var response = ResponseResult<LoginResult>.Ok(result, "User logged in successfully.");

        return Results.Ok(response);
    }

    private static async Task<IResult> RegisterAsync(RegisterCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        var response = ResponseResult<RegisterResult>.Ok(result, "User registered successfully.");

        return Results.Ok(response);
    }
}
