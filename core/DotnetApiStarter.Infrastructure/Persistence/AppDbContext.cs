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

    public DbSet<User> User => Set<User>();

    public DbSet<Role> Role => Set<Role>();

    public DbSet<UserRole> UserRole => Set<UserRole>();

    public DbSet<Product> Product => Set<Product>();

    public DbSet<Category> Category => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
