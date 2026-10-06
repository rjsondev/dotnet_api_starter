using MediatR;

namespace DotnetApiStarter.Application.Features.Products.Commands.SetProductStatus;

public sealed record SetProductStatusCommand(
    int Id,
    bool IsActive,
    int ModifiedById
) : IRequest<SetProductStatusResult>;
