using DotnetApiStarter.Application.Common.Exceptions;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;
using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace dotnetapistarter.Application.Tests.Features.Products.Commands.CreateProduct;

public class CreateProductHandlerTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ILogger<CreateProductHandler>> _loggerMock = new();

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private static CreateProductCommand CreateCommand(string sku = "SKU-001")
    {
        return new CreateProductCommand(
            Sku: sku,
            Barcode: "123456789",
            Name: "Keyboard",
            Description: "Mechanical keyboard",
            UnitPrice: 1500m,
            Reorderlevel: 10,
            CreatedById: 1);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSaveProduct()
    {
        using var dbContext = CreateDbContext();

        _cacheMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _cacheMock
            .Setup(x => x.RemoveAsync("products:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);
        var result = await handler.Handle(CreateCommand(), CancellationToken.None);
        var savedProduct = await dbContext.Product.SingleAsync(x => x.Sku == "SKU-001");

        Assert.Equal("SKU-001", savedProduct.Sku);
        Assert.Equal("Keyboard", savedProduct.Name);
        Assert.Equal("123456789", savedProduct.BarCode);
        Assert.Equal(1500m, savedProduct.UnitPrice);
        Assert.Equal(10, savedProduct.ReorderLevel);
        Assert.True(savedProduct.IsActive);
        Assert.True(savedProduct.Id > 0);
        Assert.Equal(new CreateProductResult(savedProduct.Id, "Working"), result);

        _cacheMock.Verify(x => x.RemoveAsync("products:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ShouldThrowConflictException()
    {
        using var dbContext = CreateDbContext();

        dbContext.Product.Add(new Product
        {
            Sku = "SKU-001",
            Name = "Existing keyboard"
        });

        await dbContext.SaveChangesAsync();

        var handler = new CreateProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(CreateCommand(), CancellationToken.None));
        Assert.Equal(1, await dbContext.Product.CountAsync());

        _cacheMock.Verify(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CacheUnavailable_ShouldStillSaveProduct()
    {
        using var dbContext = CreateDbContext();

        _cacheMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateProductHandler(_loggerMock.Object, dbContext, _cacheMock.Object);
        var result = await handler.Handle(CreateCommand(), CancellationToken.None);

        Assert.True(await dbContext.Product.AnyAsync(x => x.Sku == "SKU-001"));
        Assert.Equal(new CreateProductResult(1, "Unavailable"), result);

        _cacheMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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
