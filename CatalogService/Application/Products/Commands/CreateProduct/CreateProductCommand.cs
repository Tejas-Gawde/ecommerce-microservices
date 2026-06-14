namespace CatalogService.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<Guid>
{
  public required string Name { get; init; }
  public required string Description { get; init; }
  public decimal Price { get; init; }
  public int StockQuantity { get; init; }
  public Guid CategoryId { get; init; }
}