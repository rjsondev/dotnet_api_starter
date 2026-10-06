using MediatR;

namespace DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    decimal UnitPrice,
    int Reorderlevel,
    int CreatedById
) : IRequest<CreateProductResult>;
