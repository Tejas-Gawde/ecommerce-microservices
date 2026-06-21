using OrdersService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrdersService.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.ToTable("Orders");

    builder.HasKey(o => o.Id);

    builder.Property(o => o.CustomerId)
        .IsRequired();

    builder.Property(o => o.Status)
        .IsRequired()
        .HasConversion<string>()
        .HasMaxLength(50);

    builder.Property(o => o.TotalAmount)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    builder.Property(o => o.CreatedAt)
        .IsRequired();

    builder.HasMany(o => o.OrderItems)
        .WithOne(i => i.Order)
        .HasForeignKey(i => i.OrderId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(o => o.CustomerId);
    builder.HasIndex(o => o.Status);
    builder.HasIndex(o => o.CreatedAt);
  }
}