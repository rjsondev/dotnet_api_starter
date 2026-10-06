using MediatR;

namespace DotnetApiStarter.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(
    int Id
) : IRequest<DeleteProductResult>;
