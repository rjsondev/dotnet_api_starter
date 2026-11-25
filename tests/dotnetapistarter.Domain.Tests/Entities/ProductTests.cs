using DotnetApiStarter.Domain.Entities;

namespace dotnetapistarter.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Act
        var product = new Product();

        // Assert
        Assert.Equal(0, product.Id);
        Assert.Equal(string.Empty, product.Sku);
        Assert.Equal(string.Empty, product.BarCode);
        Assert.Equal(string.Empty, product.Name);
        Assert.Equal(string.Empty, product.Description);
        Assert.Null(product.CategoryId);
        Assert.Null(product.UnitPrice);
        Assert.Equal(0, product.ReorderLevel);
        Assert.False(product.IsActive);
    }

    [Fact]
    public void Properties_ShouldStoreAssignedValues()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Sku = "SKU-001",
            BarCode = "123456789",
            Name = "Keyboard",
            Description = "Mechanical keyboard",
            CategoryId = 2,
            UnitPrice = 1500.00m,
            ReorderLevel = 10,
            IsActive = true
        };

        // Assert
        Assert.Equal(1, product.Id);
        Assert.Equal("SKU-001", product.Sku);
        Assert.Equal("123456789", product.BarCode);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal("Mechanical keyboard", product.Description);
        Assert.Equal(2, product.CategoryId);
        Assert.Equal(1500.00m, product.UnitPrice);
        Assert.Equal(10, product.ReorderLevel);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void OptionalProperties_ShouldAcceptNullValues()
    {
        // Arrange
        var product = new Product
        {
            Sku = "SKU-002",
            Name = "Mouse",
            BarCode = null,
            Description = null,
            CategoryId = null,
            UnitPrice = null
        };

        // Assert
        Assert.Null(product.BarCode);
        Assert.Null(product.Description);
        Assert.Null(product.CategoryId);
        Assert.Null(product.UnitPrice);
    }
}
