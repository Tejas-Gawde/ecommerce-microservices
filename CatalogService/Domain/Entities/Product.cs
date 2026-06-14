namespace CatalogService.Domain.Entities;

public sealed class Product
{
  public Guid Id { get; private set; }
  public string Name { get; private set; }
  public string Description { get; private set; }
  public decimal Price { get; private set; }
  public int StockQuantity { get; private set; }
  public Guid CategoryId { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  public Category Category { get; private set; }

  private Product() { } // EF Core

  private Product(
      string name,
      string description,
      decimal price,
      int stockQuantity,
      Guid categoryId)
  {
    Id = Guid.NewGuid();
    Name = name;
    Description = description;
    Price = price;
    StockQuantity = stockQuantity;
    CategoryId = categoryId;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  public static Product Create(
      string name,
      string description,
      decimal price,
      int stockQuantity,
      Guid categoryId)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Product name is required.", nameof(name));

    if (price <= 0)
      throw new ArgumentException("Price must be greater than zero.", nameof(price));

    if (stockQuantity < 0)
      throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

    return new Product(name, description, price, stockQuantity, categoryId);
  }

  public void Update(
      string name,
      string description,
      decimal price,
      int stockQuantity,
      Guid categoryId)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Product name is required.", nameof(name));

    if (price <= 0)
      throw new ArgumentException("Price must be greater than zero.", nameof(price));

    if (stockQuantity < 0)
      throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

    Name = name;
    Description = description;
    Price = price;
    StockQuantity = stockQuantity;
    CategoryId = categoryId;
    UpdatedAt = DateTime.UtcNow;
  }
}