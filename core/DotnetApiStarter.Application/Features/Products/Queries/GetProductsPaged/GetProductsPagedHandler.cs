using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Application.Features.Products.Queries.GetProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotnetApiStarter.Application.Features.Products.Queries.GetProductsPaged;

public sealed class GetProductsPagedHandler : IRequestHandler<GetProductsPagedQuery, GetProductsPagedResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProductsPagedHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProductsPagedResult> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var products = _dbContext.Product.AsNoTracking();
        var totalCount = await products.CountAsync(cancellationToken);
        var offset = ((long)request.PageNumber - 1) * request.PageSize;

        IReadOnlyList<ProductDto> items = offset > int.MaxValue
            ? Array.Empty<ProductDto>()
            : await products
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .Skip((int)offset)
                .Take(request.PageSize)
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

        return new GetProductsPagedResult(request.PageNumber, request.PageSize, totalCount, items);
    }
}
