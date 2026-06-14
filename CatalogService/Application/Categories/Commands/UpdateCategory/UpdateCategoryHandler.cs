using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand>
{
  private readonly CatalogDbContext _dbContext;
  private readonly ILogger<UpdateCategoryHandler> _logger;

  public UpdateCategoryHandler(
      CatalogDbContext dbContext,
      ILogger<UpdateCategoryHandler> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
  {
    var category = await _dbContext.Categories
        .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

    if (category is null)
      throw new KeyNotFoundException($"Category with ID {command.Id} not found.");

    category.Update(command.Name);
    await _dbContext.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
        "Category updated: {CategoryId} - {CategoryName}",
        category.Id,
        category.Name);
  }
}