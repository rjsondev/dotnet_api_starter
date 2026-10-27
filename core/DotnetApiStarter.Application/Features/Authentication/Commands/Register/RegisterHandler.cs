using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Application.Features.Authentication.Commands.Login;
using DotnetApiStarter.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Authentication.Commands.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginHandler> _logger;

    public RegisterHandler(ILogger<LoginHandler> logger, IApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling register request for username: {Username}", request.Username);

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        var usernameExists = await _dbContext.User.AnyAsync(u => u.Username == username, cancellationToken);

        if (usernameExists)
            throw new ConflictException("Username is already taken.");

        var emailExists = await _dbContext.User.AnyAsync(u => u.Email == email, cancellationToken);

        if (emailExists)
            throw new ConflictException("Email is already registered.");

        var regularRole = await _dbContext.Role.SingleOrDefaultAsync(r => r.Name == "Regular", cancellationToken);

        if (regularRole is null)
            throw new InvalidOperationException("The Regular role has not been seeded.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = regularRole
        });

        _dbContext.User.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);
        return new RegisterResult(
            user.Id,
            user.Username,
            user.Email);
    }
}
