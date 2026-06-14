using CatalogService.Application.Categories.Dtos;

namespace CatalogService.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery : IRequest<CategoryDto>
{
  public Guid Id { get; init; }
}