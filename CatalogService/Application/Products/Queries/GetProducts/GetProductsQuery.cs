using CatalogService.Application.Products.Dtos;

namespace CatalogService.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<List<ProductDto>>
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
  public string? SearchTerm { get; init; }
  public Guid? CategoryId { get; init; }
  public string? SortBy { get; init; }
  public bool SortDescending { get; init; } = false;
}