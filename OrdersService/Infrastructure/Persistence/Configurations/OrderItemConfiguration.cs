using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
  public void Configure(EntityTypeBuilder<OrderItem> builder)
  {
    builder.ToTable("OrderItems");

    builder.HasKey(i => i.Id);

    builder.Property(i => i.ProductId)
        .IsRequired();

    builder.Property(i => i.Quantity)
        .IsRequired();

    builder.Property(i => i.Price)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    builder.HasIndex(i => i.ProductId);
    builder.HasIndex(i => i.OrderId);
  }
}