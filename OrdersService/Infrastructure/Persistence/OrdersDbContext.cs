using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Persistence;

public sealed class OrdersDbContext : DbContext
{
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();

  public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
      : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
  }
}