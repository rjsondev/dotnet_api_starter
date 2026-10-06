using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateProductHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Product.AnyAsync(x => x.Sku == request.Sku, cancellationToken);

        if (exists)
        {
            //Exception handling for duplicate SKU
        }

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

        return new CreateProductResult(product.Id);
    }
}
