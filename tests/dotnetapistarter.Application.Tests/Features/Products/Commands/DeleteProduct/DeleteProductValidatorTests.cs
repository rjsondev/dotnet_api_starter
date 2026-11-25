using DotnetApiStarter.Application.Features.Products.Commands.DeleteProduct;
using FluentValidation.TestHelper;

namespace dotnetapistarter.Application.Tests.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductValidatorTests
{
    private readonly DeleteProductValidator _validator = new();

    [Fact]
    public void Validate_ValidId_ShouldNotHaveErrors()
    {
        var command = new DeleteProductCommand(1);

        var result = _validator.TestValidate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ZeroId_ShouldNotHaveErrors()
    {
        var command = new DeleteProductCommand(0);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_NegativeId_ShouldHaveError()
    {
        var command = new DeleteProductCommand(-1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
