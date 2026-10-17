using DotnetApiStarter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetProductsHandler> _logger;

    public GetProductsHandler(IApplicationDbContext dbContext, ILogger<GetProductsHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Product.AsNoTracking()
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
    }
}
