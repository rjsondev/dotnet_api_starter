using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Extensions;
using DotnetApiStarter.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler : IRequestHandler<DeleteProductCommand, DeleteProductResult>
{
    private const string CacheKey = "products:all";
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeleteProductHandler> _logger;    

    public DeleteProductHandler(ILogger<DeleteProductHandler> logger
        , IApplicationDbContext dbContext
        , ICacheService cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<DeleteProductResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product with ID {ProductId}", request.Id);

        var product = await _dbContext.Product.FindAsync(request.Id, cancellationToken);

        if (product is null)
            throw new NotFoundException($"Product {request.Id} was not found.");

        _dbContext.Product.Remove(product);

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product with ID {ProductId} has been deleted", request.Id);

        var cacheResult = await _cache.RemoveFromCacheAsync(CacheKey, product, cancellationToken);
        return new DeleteProductResult(product.Id, cacheResult.IsAvailable ? "Working" : "Unavailable");
    }
}
