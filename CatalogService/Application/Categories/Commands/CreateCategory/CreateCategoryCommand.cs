namespace CatalogService.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand : IRequest<Guid>
{
  public required string Name { get; init; }
}