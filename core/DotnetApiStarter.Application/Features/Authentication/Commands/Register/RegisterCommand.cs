using MediatR;

namespace DotnetApiStarter.Application.Features.Authentication.Commands.Register;

public sealed record RegisterCommand(string Username, string Email, string Password) 
    : IRequest<RegisterResult>;
