using DotnetApiStarter.Api.Common.Responses;
using DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;
using DotnetApiStarter.Application.Features.Products.Commands.DeleteProduct;
using DotnetApiStarter.Application.Features.Products.Commands.SetProductStatus;
using DotnetApiStarter.Application.Features.Products.Commands.UpdateProduct;
using DotnetApiStarter.Application.Features.Products.Queries.GetProducts;
using MediatR;

namespace DotnetApiStarter.Api.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/product").WithTags("Products");

        group.MapPost("/", CreateProduct);
        group.MapGet("/", GetProducts);
        group.MapPut("/{id:int}", UpdateProduct);
        group.MapPatch("/{id:int}/status", SetProductStatus);
        group.MapDelete("/{id:int}", DeleteProduct);

        return app;
    }

    private static async Task<IResult> CreateProduct(CreateProductCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        var response = ResponseResult<CreateProductResult>.Ok(result, "Product created successfully.");

        return Results.Created(string.Empty, response);
    }
    
    private static async Task<IResult> GetProducts(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductsQuery(), cancellationToken);

        return Results.Ok(ResponseResult<IReadOnlyList<ProductDto>>.Ok(result, "Products retrieved successfully."));
    }

    private static async Task<IResult> UpdateProduct(int id, UpdateProductCommand command, ISender sender, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        //await sender.Send(command, cancellationToken);
        //return Results.NoContent();

        var result = await sender.Send(command, cancellationToken);
        var response = ResponseResult<UpdateProductResult>.Ok(result, "Product updated successfully.");

        return Results.Ok(response);
    }

    private static async Task<IResult> SetProductStatus(int id, SetProductStatusCommand command, ISender sender, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        //await sender.Send(command, cancellationToken);
        //return Results.NoContent();

        var result = await sender.Send(command, cancellationToken);
        var response = ResponseResult<SetProductStatusResult>.Ok(result, "Product status updated successfully.");

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteProduct(int id, ISender sender, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);

        //await sender.Send(command, cancellationToken);
        //return Results.NoContent();

        var result = await sender.Send(command, cancellationToken);
        var response = ResponseResult<DeleteProductResult>.Ok(result, "Product deleted successfully.");

        return Results.Ok(response);
    }
}
