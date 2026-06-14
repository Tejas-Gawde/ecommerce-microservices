using CatalogService.Application.Categories.Dtos;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
  private readonly CatalogDbContext _dbContext;

  public GetCategoryByIdHandler(CatalogDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
  {
    var category = await _dbContext.Categories
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

    if (category is null)
      throw new KeyNotFoundException($"Category with ID {request.Id} not found.");

    return new CategoryDto
    {
      Id = category.Id,
      Name = category.Name
    };
  }
}