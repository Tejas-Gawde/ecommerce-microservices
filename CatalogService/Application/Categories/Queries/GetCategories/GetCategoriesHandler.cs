using CatalogService.Application.Categories.Dtos;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
  private readonly CatalogDbContext _dbContext;

  public GetCategoriesHandler(CatalogDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
  {
    var categories = await _dbContext.Categories
        .AsNoTracking()
        .OrderBy(c => c.Name)
        .ToListAsync(cancellationToken);

    return categories.Select(c => new CategoryDto
    {
      Id = c.Id,
      Name = c.Name
    }).ToList();
  }
}