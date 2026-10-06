using DotnetApiStarter.Api.Common.Responses;
using DotnetApiStarter.Application.Features.Products.Commands.CreateProduct;
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
}
