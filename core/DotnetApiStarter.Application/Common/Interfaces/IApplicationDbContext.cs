using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetApiStarter.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Product { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
