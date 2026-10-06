using MediatR;

namespace DotnetApiStarter.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
