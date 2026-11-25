using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Application.Features.Products.Commands.UpdateProduct;
using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace dotnetapistarter.Application.Tests.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandlerTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ILogger<UpdateProductHandler>> _loggerMock = new();

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private static Product CreateProduct(
        string sku = "SKU-001",
        string name = "Keyboard")
    {
        return new Product
        {
            Sku = sku,
            Name = name,
            UnitPrice = 1000m,
            IsActive = true,
            ReorderLevel = 5
        };
    }

    private static UpdateProductCommand CreateCommand(
        int id,
        string sku = "SKU-001",
        string name = "Updated Keyboard")
    {
        return new UpdateProductCommand(
            Id: id,
            Sku: sku,
            Barcode: "987654321",
            Name: name,
            Description: "Updated description",
            UnitPrice: 2000m,
            Reorderlevel: 10,
            ModifiedById: 2);
    }

    private void SetupCacheAvailable()
    {
        _cacheMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cacheMock
            .Setup(x => x.RemoveAsync("products:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateProduct()
    {
        using var dbContext = CreateDbContext();

        var product = CreateProduct();
        dbContext.Product.Add(product);
        await dbContext.SaveChangesAsync();

        SetupCacheAvailable();

        var handler = new UpdateProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        var result = await handler.Handle(CreateCommand(product.Id), CancellationToken.None);

        var updatedProduct = await dbContext.Product.FirstAsync(x => x.Id == product.Id);

        Assert.Equal("SKU-001", updatedProduct.Sku);
        Assert.Equal("Updated Keyboard", updatedProduct.Name);
        Assert.Equal("987654321", updatedProduct.BarCode);
        Assert.Equal("Updated description", updatedProduct.Description);
        Assert.Equal(2000m, updatedProduct.UnitPrice);
        Assert.Equal(10, updatedProduct.ReorderLevel);
        Assert.Equal(2, updatedProduct.ModifiedById);

        Assert.Equal(new UpdateProductResult(product.Id, "Working"), result);

        _cacheMock.Verify(
            x => x.RemoveAsync(
                "products:all",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowNotFoundException()
    {
        using var dbContext = CreateDbContext();

        var handler = new UpdateProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(CreateCommand(999), CancellationToken.None));

        _cacheMock.Verify(
            x => x.IsAvailableAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ShouldThrowConflictException()
    {
        using var dbContext = CreateDbContext();

        dbContext.Product.AddRange(
            CreateProduct("SKU-001", "Keyboard"),
            CreateProduct("SKU-002", "Mouse"));

        await dbContext.SaveChangesAsync();

        var products = await dbContext.Product.ToListAsync();
        var targetProduct = products.First(x => x.Sku == "SKU-001");

        var handler = new UpdateProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(
                CreateCommand(targetProduct.Id, sku: "SKU-002"),
                CancellationToken.None));

        var unchangedProduct = await dbContext.Product
            .FirstAsync(x => x.Id == targetProduct.Id);

        Assert.Equal("SKU-001", unchangedProduct.Sku);
        Assert.Equal("Keyboard", unchangedProduct.Name);

        _cacheMock.Verify(
            x => x.IsAvailableAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CacheUnavailable_ShouldStillUpdateProduct()
    {
        using var dbContext = CreateDbContext();

        var product = CreateProduct();
        dbContext.Product.Add(product);
        await dbContext.SaveChangesAsync();

        _cacheMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdateProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        var result = await handler.Handle(CreateCommand(product.Id), CancellationToken.None);

        var updatedProduct = await dbContext.Product.FirstAsync(x => x.Id == product.Id);

        Assert.Equal("Updated Keyboard", updatedProduct.Name);
        Assert.Equal("Unavailable", result.CacheStatus);

        _cacheMock.Verify(
            x => x.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<User> User => Set<User>();

        public DbSet<Role> Role => Set<Role>();

        public DbSet<Product> Product => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<User>();
            modelBuilder.Ignore<Role>();
            modelBuilder.Ignore<UserRole>();
        }
    }
}
