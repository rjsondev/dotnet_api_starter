namespace DotnetApiStarter.Application.Features.Authentication.Commands.Login;

public sealed record LoginResult(string AccessToken, DateTime ExpiresAtUtc);
