using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    private const string CacheKey = "products:all";
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateProductHandler> _logger;    

    public CreateProductHandler(ILogger<CreateProductHandler> logger
        , IApplicationDbContext dbContext
        , ICacheService cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with SKU {Sku}", request.Sku);

        var exists = await _dbContext.Product.AnyAsync(x => x.Sku == request.Sku, cancellationToken);

        if (exists)
            throw new ConflictException($"Product SKU '{request.Sku}' already exists.");

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            BarCode = request.Barcode,
            Description = request.Description,
            UnitPrice = request.UnitPrice,
            IsActive = true,
            ReorderLevel = request.Reorderlevel,
            CreatedDate = DateTime.UtcNow,
            CreatedById = request.CreatedById,
        };

        _dbContext.Product.Add(product);

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        // Invalidate the product cache
        await _cache.RemoveAsync(CacheKey, cancellationToken);

        _logger.LogInformation("Product {ProductId} created", product.Id);
        return new CreateProductResult(product.Id);
    }
}
