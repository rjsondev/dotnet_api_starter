namespace DotnetApiStarter.Application.Common.Authentication;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);
