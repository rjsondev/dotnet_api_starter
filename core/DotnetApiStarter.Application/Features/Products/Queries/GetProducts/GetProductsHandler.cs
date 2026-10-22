using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private const string CacheKey = "products:all";
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _dbContext; 

    public GetProductsHandler(IApplicationDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // # (1) Try to get products from cache
        var cachedProducts = await _cache.GetAsync<List<ProductDto>>(CacheKey, cancellationToken);

        // # (2) If products are found in cache, return them
        if (cachedProducts is not null)
            return cachedProducts;

        // # (3) If products are not found in cache, retrieve them from the database
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

        // # (4) Store the retrieved products in cache for future requests
        await _cache.SetAsync(CacheKey, products, TimeSpan.FromMinutes(5), cancellationToken);

        return products;
    }
}
