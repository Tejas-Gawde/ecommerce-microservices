using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
  private readonly CatalogDbContext _dbContext;
  private readonly ILogger<DeleteCategoryHandler> _logger;

  public DeleteCategoryHandler(
      CatalogDbContext dbContext,
      ILogger<DeleteCategoryHandler> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
  {
    var category = await _dbContext.Categories
        .Include(c => c.Products)
        .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

    if (category is null)
      throw new KeyNotFoundException($"Category with ID {command.Id} not found.");

    if (category.Products.Any())
      throw new InvalidOperationException(
          $"Cannot delete category '{category.Name}' because it contains {category.Products.Count} product(s).");

    _dbContext.Categories.Remove(category);
    await _dbContext.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
        "Category deleted: {CategoryId} - {CategoryName}",
        command.Id,
        category.Name);
  }
}