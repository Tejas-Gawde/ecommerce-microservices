using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.ToTable("Products");

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Name)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(p => p.Description)
        .HasMaxLength(2000);

    builder.Property(p => p.Price)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    builder.Property(p => p.StockQuantity)
        .IsRequired();

    builder.Property(p => p.CreatedAt)
        .IsRequired();

    builder.Property(p => p.UpdatedAt)
        .IsRequired();

    builder.HasOne(p => p.Category)
        .WithMany(c => c.Products)
        .HasForeignKey(p => p.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(p => p.Name);
    builder.HasIndex(p => p.CategoryId);
  }
}