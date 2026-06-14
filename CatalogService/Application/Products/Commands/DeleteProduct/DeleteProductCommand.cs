namespace CatalogService.Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand : IRequest
{
  public Guid Id { get; init; }
}