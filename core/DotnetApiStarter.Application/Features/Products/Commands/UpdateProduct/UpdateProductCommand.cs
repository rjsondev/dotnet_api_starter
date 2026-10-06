using MediatR;

namespace DotnetApiStarter.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    int Id,
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    decimal UnitPrice,
    int Reorderlevel,
    int ModifiedById
) : IRequest<UpdateProductResult>;
