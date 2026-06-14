namespace CatalogService.Application.Products.Dtos;

public sealed class ProductDto
{
  public Guid Id { get; init; }
  public required string Name { get; init; }
  public required string Description { get; init; }
  public decimal Price { get; init; }
  public int StockQuantity { get; init; }
  public Guid CategoryId { get; init; }
  public required string CategoryName { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
}