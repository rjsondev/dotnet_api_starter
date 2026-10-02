using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetApiStarter.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Product => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
