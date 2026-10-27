using DotnetApiStarter.Application.Common.Extensions;
using DotnetApiStarter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotnetApiStarter.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResult>
{
    private const string CacheKey = "products:all";
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _dbContext; 

    public GetProductsHandler(IApplicationDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<GetProductsResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // # (1) Try to get products from cache
        var cacheResult = await _cache.GetFromCacheAsync<List<ProductDto>>(CacheKey, cancellationToken);
        if (cacheResult.Value is not null)
        {
            return new GetProductsResult(cacheResult.Value, IsFromCache: true, CacheStatus: "Working");
        }

        // # (2) If products are not found in cache, retrieve them from the database
        var products = await _dbContext.Product.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductDto(
                x.Id,
                x.Sku,
                x.BarCode,
                x.Name,
                x.Description,
                x.CategoryId,
                x.UnitPrice,
                x.IsActive,
                x.ReorderLevel))
            .ToListAsync(cancellationToken);

        // # (3) Store the retrieved products in cache for future requests
        cacheResult = await _cache.WriteToCacheAsync(CacheKey, products, TimeSpan.FromMinutes(5), cancellationToken);

        return new GetProductsResult(products
            , IsFromCache: false
            , CacheStatus: cacheResult.IsAvailable ? "Working" : "Unavailable");
    }
}
