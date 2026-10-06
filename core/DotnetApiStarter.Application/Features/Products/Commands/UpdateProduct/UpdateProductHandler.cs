using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotnetApiStarter.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateProductHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Product.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product {request.Id} was not found.");
        }

        var duplicateSku = await _dbContext.Product.AnyAsync(
            x => x.Sku == request.Sku &&
            x.Id != request.Id,
            cancellationToken);

        if (duplicateSku)
        {
            throw new ConflictException($"Product SKU '{request.Sku}' already exists.");
        }

        product.Sku = request.Sku;
        product.BarCode = request.Barcode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.UnitPrice = request.UnitPrice;
        product.ReorderLevel = request.Reorderlevel;
        product.ModifiedDate = DateTime.UtcNow;
        product.ModifiedById = request.ModifiedById;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult(product.Id);
    }
}
