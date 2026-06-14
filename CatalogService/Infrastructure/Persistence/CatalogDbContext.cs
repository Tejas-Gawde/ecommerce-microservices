using CatalogService.Domain.Entities;

namespace CatalogService.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext
{
  public DbSet<Product> Products => Set<Product>();
  public DbSet<Category> Categories => Set<Category>();

  public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
      : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
  }
}