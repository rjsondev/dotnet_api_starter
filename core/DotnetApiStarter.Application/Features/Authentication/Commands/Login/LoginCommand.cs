using MediatR;

namespace DotnetApiStarter.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(string Username,string Password) 
    : IRequest<LoginResult>;
