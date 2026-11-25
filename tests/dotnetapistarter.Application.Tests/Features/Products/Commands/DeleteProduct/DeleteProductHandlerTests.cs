using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Application.Features.Products.Commands.DeleteProduct;
using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace dotnetapistarter.Application.Tests.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandlerTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ILogger<DeleteProductHandler>> _loggerMock = new();

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private static Product CreateProduct()
    {
        return new Product
        {
            Sku = "SKU-001",
            Name = "Keyboard",
            UnitPrice = 1500m,
            IsActive = true,
            ReorderLevel = 5
        };
    }

    private void SetupCacheAvailable()
    {
        _cacheMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cacheMock
            .Setup(x => x.RemoveAsync("products:all",It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteProduct()
    {
        using var dbContext = CreateDbContext();

        var product = CreateProduct();
        dbContext.Product.Add(product);
        await dbContext.SaveChangesAsync();

        SetupCacheAvailable();

        var handler = new DeleteProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        var deletedProduct = await dbContext.Product.FindAsync(new object?[] { product.Id });

        Assert.Null(deletedProduct);

        Assert.Equal(new DeleteProductResult(product.Id, "Working"), result);

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

        var handler = new DeleteProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new DeleteProductCommand(999),
                CancellationToken.None));

        _cacheMock.Verify(
            x => x.IsAvailableAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CacheUnavailable_ShouldStillDeleteProduct()
    {
        using var dbContext = CreateDbContext();

        var product = CreateProduct();
        dbContext.Product.Add(product);
        await dbContext.SaveChangesAsync();

        _cacheMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new DeleteProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        Assert.False(await dbContext.Product.AnyAsync(x => x.Id == product.Id));

        Assert.Equal("Unavailable", result.CacheStatus);
        Assert.Equal(product.Id, result.Id);

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
