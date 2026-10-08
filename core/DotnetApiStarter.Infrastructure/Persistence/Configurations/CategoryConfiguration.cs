using DotnetApiStarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetApiStarter.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category", "Reference");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.Description)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(c => c.DeletedDate == null || c.DeletedDate == DateTimeOffset.MinValue);
    }
}
