using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetApiStarter.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // 1. Table Mapping
        builder.ToTable("Product", "Reference");

        // 2. Keys & Identity
        builder.HasKey(p => p.Id);

        // 3. Unique Identifiers & Indexes
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.BarCode)
            .HasColumnType("nvarchar(MAX)")
            .IsRequired(false);

        // 4. Content Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(p => p.Description)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        // 5. Global Query Filter for Soft Delete
        // Automatically filters out deleted records on all SELECT queries
        builder.HasQueryFilter(p => p.DeletedDate == null || p.DeletedDate == DateTimeOffset.MinValue);
    }
}
