using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Extensions;
using DotnetApiStarter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetApiStarter.Application.Features.Products.Commands.SetProductStatus;

public sealed class SetProductStatusHandler : IRequestHandler<SetProductStatusCommand, SetProductStatusResult>
{
    private const string CacheKey = "products:all";
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<SetProductStatusHandler> _logger;    

    public SetProductStatusHandler(ILogger<SetProductStatusHandler> logger
        , IApplicationDbContext dbContext
        , ICacheService cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<SetProductStatusResult> Handle(SetProductStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting product status for product with ID {ProductId} to {IsActive}", request.Id, request.IsActive);

        var product = await _dbContext.Product.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product is null)
            throw new NotFoundException($"Product {request.Id} was not found.");

        product.IsActive = request.IsActive;
        product.ModifiedDate = DateTime.UtcNow;
        product.ModifiedById = request.ModifiedById;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product status for product with ID {ProductId} has been set to {IsActive}", request.Id, request.IsActive);

        var cacheResult = await _cache.RemoveFromCacheAsync(CacheKey, product, cancellationToken);
        return new SetProductStatusResult(product.Id, cacheResult.IsAvailable ? "Working" : "Unavailable");
    }
}
