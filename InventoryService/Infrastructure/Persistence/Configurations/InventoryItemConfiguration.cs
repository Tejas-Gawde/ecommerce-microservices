using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
  public void Configure(EntityTypeBuilder<InventoryItem> builder)
  {
    builder.ToTable("InventoryItems");

    builder.HasKey(i => i.Id);

    builder.Property(i => i.ProductId)
        .IsRequired();

    builder.Property(i => i.AvailableQuantity)
        .IsRequired();

    builder.Property(i => i.ReservedQuantity)
        .IsRequired();

    builder.Property(i => i.LastUpdated)
        .IsRequired();

    builder.HasIndex(i => i.ProductId)
        .IsUnique();

    // Ensure non-negative quantities using check constraints
    builder.ToTable(t => t.HasCheckConstraint(
        "CK_InventoryItems_AvailableQuantity",
        "\"AvailableQuantity\" >= 0"));

    builder.ToTable(t => t.HasCheckConstraint(
        "CK_InventoryItems_ReservedQuantity",
        "\"ReservedQuantity\" >= 0"));
  }
}