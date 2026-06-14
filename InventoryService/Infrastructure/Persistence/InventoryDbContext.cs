using InventoryService.Domain.Entities;

namespace InventoryService.Infrastructure.Persistence;

public sealed class InventoryDbContext : DbContext
{
  public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

  public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
      : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
  }
}