using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, UpdateProductResult>
{
    private const string CacheKey = "products:all";
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateProductHandler> _logger;    

    public UpdateProductHandler(ILogger<UpdateProductHandler> logger
        , IApplicationDbContext dbContext
        , ICacheService cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product with ID {ProductId}", request.Id);

        var product = await _dbContext.Product.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product is null)
            throw new NotFoundException($"Product {request.Id} was not found.");

        var duplicateSku = await _dbContext.Product.AnyAsync(
            x => x.Sku == request.Sku &&
            x.Id != request.Id,
            cancellationToken);

        if (duplicateSku)
            throw new ConflictException($"Product SKU '{request.Sku}' already exists.");

        product.Sku = request.Sku;
        product.BarCode = request.Barcode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.UnitPrice = request.UnitPrice;
        product.ReorderLevel = request.Reorderlevel;
        product.ModifiedDate = DateTime.UtcNow;
        product.ModifiedById = request.ModifiedById;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKey, cancellationToken);

        _logger.LogInformation("Product with ID {ProductId} updated successfully", product.Id);
        return new UpdateProductResult(product.Id);
    }
}
