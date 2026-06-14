namespace CatalogService.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand : IRequest
{
  public Guid Id { get; init; }
  public required string Name { get; init; }
  public required string Description { get; init; }
  public decimal Price { get; init; }
  public int StockQuantity { get; init; }
  public Guid CategoryId { get; init; }
}