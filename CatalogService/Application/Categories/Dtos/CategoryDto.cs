namespace CatalogService.Application.Categories.Dtos;

public sealed class CategoryDto
{
  public Guid Id { get; init; }
  public required string Name { get; init; }
}