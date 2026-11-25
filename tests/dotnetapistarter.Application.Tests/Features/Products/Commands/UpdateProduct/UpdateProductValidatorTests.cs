using DotnetApiStarter.Application.Features.Products.Commands.UpdateProduct;
using FluentValidation.TestHelper;

namespace dotnetapistarter.Application.Tests.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    private static UpdateProductCommand CreateValidCommand(
        string name = "Keyboard",
        string? description = "Mechanical keyboard",
        decimal unitPrice = 1500m)
    {
        return new UpdateProductCommand(
            Id: 1,
            Sku: "SKU-001",
            Barcode: "123456789",
            Name: name,
            Description: description,
            UnitPrice: unitPrice,
            Reorderlevel: 10,
            ModifiedById: 1);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveErrors()
    {
        var result = _validator.TestValidate(CreateValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveError()
    {
        var result = _validator.TestValidate(CreateValidCommand(name: ""));

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceeds200Characters_ShouldHaveError()
    {
        var result = _validator.TestValidate(CreateValidCommand(name: new string('A', 201)));

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExactly200Characters_ShouldBeValid()
    {
        var result = _validator.TestValidate(CreateValidCommand(name: new string('A', 200)));

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionExceeds500Characters_ShouldHaveError()
    {
        var result = _validator.TestValidate(CreateValidCommand(description: new string('A', 501)));

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NullDescription_ShouldBeValid()
    {
        var result = _validator.TestValidate(CreateValidCommand(description: null));

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NegativeUnitPrice_ShouldHaveError()
    {
        var result = _validator.TestValidate(CreateValidCommand(unitPrice: -1m));

        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact]
    public void Validate_ZeroUnitPrice_ShouldBeValid()
    {
        var result = _validator.TestValidate(CreateValidCommand(unitPrice: 0m));

        result.ShouldNotHaveValidationErrorFor(x => x.UnitPrice);
    }
}
