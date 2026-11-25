using DotnetApiStarter.Application.Common.Caching;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Application.Features.Products.Queries.GetProducts;
using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace dotnetapistarter.Application.Tests.Features.Products.Queries.GetProducts;

public sealed class GetProductsHandlerTests
{
    private readonly Mock<ICacheService> _cacheMock = new();

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private static Product CreateProduct(string sku, string name, decimal unitPrice = 100m)
    {
        return new Product
        {
            Sku = sku,
            Name = name,
            UnitPrice = unitPrice,
            IsActive = true,
            ReorderLevel = 5
        };
    }

    [Fact]
    public async Task Handle_CacheHit_ShouldReturnCachedProducts()
    {
        using var dbContext = CreateDbContext();

        var cachedProducts = new List<ProductDto>
        {
            new(10, "SKU-010", null, "Keyboard", null, null, 1500m, true, 5)
        };

        _cacheMock.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _cacheMock.Setup(x => x.GetAsync<List<ProductDto>>("products:all", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheResult<List<ProductDto>>(cachedProducts, true));

        var handler = new GetProductsHandler(dbContext, _cacheMock.Object);

        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        Assert.True(result.IsFromCache);
        Assert.Equal("Working", result.CacheStatus);
        Assert.Equal(cachedProducts, result.Products);

        _cacheMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<List<ProductDto>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CacheMiss_ShouldReturnDatabaseProductsAndCacheThem()
    {
        using var dbContext = CreateDbContext();

        dbContext.Product.AddRange(
            CreateProduct("SKU-002", "Mouse", 500m),
            CreateProduct("SKU-001", "Keyboard", 1500m));

        await dbContext.SaveChangesAsync();

        _cacheMock.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _cacheMock.Setup(x => x.GetAsync<List<ProductDto>>("products:all",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CacheResult<List<ProductDto>>(
                null, true));

        _cacheMock.Setup(x => x.SetAsync<List<ProductDto>>("products:all",
                It.IsAny<List<ProductDto>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new GetProductsHandler(dbContext, _cacheMock.Object);

        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        Assert.False(result.IsFromCache);
        Assert.Equal("Working", result.CacheStatus);
        Assert.Equal(2, result.Products!.Count);
        Assert.Equal("Keyboard", result.Products[0].Name);
        Assert.Equal("Mouse", result.Products[1].Name);

        _cacheMock.Verify(
            x => x.SetAsync<List<ProductDto>>(
                "products:all",
                It.IsAny<List<ProductDto>>(),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(5)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CacheUnavailable_ShouldStillReturnDatabaseProducts()
    {
        using var dbContext = CreateDbContext();

        dbContext.Product.Add(CreateProduct("SKU-001", "Keyboard"));
        await dbContext.SaveChangesAsync();

        _cacheMock.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new GetProductsHandler(dbContext, _cacheMock.Object);

        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        Assert.False(result.IsFromCache);
        Assert.Equal("Unavailable", result.CacheStatus);
        Assert.Single(result.Products!);
        Assert.Equal("Keyboard", result.Products[0].Name);

        _cacheMock.Verify(
            x => x.SetAsync<List<ProductDto>>(
                It.IsAny<string>(),
                It.IsAny<List<ProductDto>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ShouldReturnEmptyList()
    {
        using var dbContext = CreateDbContext();

        _cacheMock.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new GetProductsHandler(dbContext, _cacheMock.Object);

        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        Assert.NotNull(result.Products);
        Assert.Empty(result.Products);
        Assert.False(result.IsFromCache);
        Assert.Equal("Unavailable", result.CacheStatus);
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

            // These entities aren't needed for product-query tests.
            modelBuilder.Ignore<User>();
            modelBuilder.Ignore<Role>();
        }
    }
}
