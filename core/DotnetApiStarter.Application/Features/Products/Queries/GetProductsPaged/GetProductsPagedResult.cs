using DotnetApiStarter.Application.Features.Products.Queries.GetProducts;

namespace DotnetApiStarter.Application.Features.Products.Queries.GetProductsPaged;

public sealed record GetProductsPagedResult(
    int PageNumber,
    int PageSize,
    int TotalCount,
    IReadOnlyList<ProductDto> Items);
