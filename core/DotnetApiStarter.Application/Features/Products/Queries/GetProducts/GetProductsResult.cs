namespace DotnetApiStarter.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsResult(
    IReadOnlyList<ProductDto>? Products, 
    bool IsFromCache, 
    string CacheStatus);
