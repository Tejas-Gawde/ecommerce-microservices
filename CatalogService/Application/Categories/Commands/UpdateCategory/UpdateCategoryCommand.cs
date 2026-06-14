namespace CatalogService.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand : IRequest
{
  public Guid Id { get; init; }
  public required string Name { get; init; }
}