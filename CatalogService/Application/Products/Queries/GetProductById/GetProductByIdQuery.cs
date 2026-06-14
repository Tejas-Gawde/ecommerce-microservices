using CatalogService.Application.Products.Dtos;

namespace CatalogService.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery : IRequest<ProductDto>
{
  public Guid Id { get; init; }
}