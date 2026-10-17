using DotnetApiStarter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Authentication.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(ILogger<LoginHandler> logger, IApplicationDbContext dbContext, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling login request for username: {Username}", request.Username);

        var user = await _dbContext.User
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(
                u => u.Username == request.Username,
                cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToArray();
        var token = _jwtTokenService.GenerateToken(user.Id, user.Username, roles);

        _logger.LogInformation("User {Username} logged in successfully. Token expires at {ExpiresAtUtc}", user.Username, token.ExpiresAtUtc);
        return new LoginResult(
            token.AccessToken,
            token.ExpiresAtUtc);
    }
}
