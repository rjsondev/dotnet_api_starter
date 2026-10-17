using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetApiStarter.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> User { get; }

    DbSet<Role> Role { get; }

    DbSet<Product> Product { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
