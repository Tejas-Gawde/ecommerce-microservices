namespace CatalogService.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand : IRequest
{
  public Guid Id { get; init; }
}