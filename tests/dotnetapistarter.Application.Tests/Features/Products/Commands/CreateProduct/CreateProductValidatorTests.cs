using DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;
using FluentValidation.TestHelper;

namespace dotnetapistarter.Application.Tests.Features.Products.Commands.CreateProduct;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    private static CreateProductCommand CreateValidCommand(
        string sku = "SKU-001",
        string name = "Keyboard",
        string? description = "Mechanical keyboard",
        decimal unitPrice = 1500m)
    {
        return new CreateProductCommand(
            Sku: sku,
            Barcode: "123456789",
            Name: name,
            Description: description,
            UnitPrice: unitPrice,
            Reorderlevel: 10,
            CreatedById: 1);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptySku_ShouldHaveError()
    {
        var command = CreateValidCommand(sku: "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Sku);
    }

    [Fact]
    public void Validate_SkuExceeds50Characters_ShouldHaveError()
    {
        var command = CreateValidCommand(sku: new string('A', 51));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Sku);
    }

    [Fact]
    public void Validate_SkuExactly50Characters_ShouldBeValid()
    {
        var command = CreateValidCommand(sku: new string('A', 50));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Sku);
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveError()
    {
        var command = CreateValidCommand(name: "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceeds200Characters_ShouldHaveError()
    {
        var command = CreateValidCommand(name: new string('A', 201));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExactly200Characters_ShouldBeValid()
    {
        var command = CreateValidCommand(name: new string('A', 200));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionExceeds500Characters_ShouldHaveError()
    {
        var command = CreateValidCommand(
            description: new string('A', 501));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NullDescription_ShouldBeValid()
    {
        var command = CreateValidCommand(description: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NegativeUnitPrice_ShouldHaveError()
    {
        var command = CreateValidCommand(unitPrice: -1m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact]
    public void Validate_ZeroUnitPrice_ShouldBeValid()
    {
        var command = CreateValidCommand(unitPrice: 0m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UnitPrice);
    }
}
