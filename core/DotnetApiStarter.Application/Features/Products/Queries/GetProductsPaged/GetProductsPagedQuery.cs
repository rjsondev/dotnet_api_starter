using MediatR;

namespace DotnetApiStarter.Application.Features.Products.Queries.GetProductsPaged;

public sealed record GetProductsPagedQuery(int PageNumber = 1, int PageSize = 20) : IRequest<GetProductsPagedResult>;
