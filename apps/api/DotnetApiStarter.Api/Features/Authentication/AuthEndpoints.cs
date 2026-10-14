using DotnetApiStarter.Api.Common.Responses;
using DotnetApiStarter.Application.Features.Authentication.Commands.Login;
using DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApiStarter.Api.Features.Authentication;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication").AllowAnonymous();

        group.MapPost("/login", LoginAsync);

        return app;
    }

    private static async Task<IResult> LoginAsync([FromBody] LoginCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        var response = ResponseResult<LoginResult>.Ok(result, "User logged in successfully.");

        return Results.Ok(result);
    }
}
