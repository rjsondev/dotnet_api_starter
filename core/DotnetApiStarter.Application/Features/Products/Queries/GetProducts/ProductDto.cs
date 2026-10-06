namespace DotnetApiStarter.Application.Features.Products.Queries.GetProducts;

public sealed record ProductDto(
    int Id,
    string Sku,
    string? BarCode,
    string Name,
    string? Description,
    int? CategoryId,
    decimal? UnitPrice,
    bool IsActive,
    int ReorderLevel
);
