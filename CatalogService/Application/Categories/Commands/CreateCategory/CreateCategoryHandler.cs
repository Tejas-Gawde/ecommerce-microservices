using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
  private readonly CatalogDbContext _dbContext;
  private readonly ILogger<CreateCategoryHandler> _logger;

  public CreateCategoryHandler(
      CatalogDbContext dbContext,
      ILogger<CreateCategoryHandler> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task<Guid> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
  {
    var category = Category.Create(command.Name);

    _dbContext.Categories.Add(category);
    await _dbContext.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
        "Category created: {CategoryId} - {CategoryName}",
        category.Id,
        category.Name);

    return category.Id;
  }
}