using CatalogService.Application.Categories.Dtos;

namespace CatalogService.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<List<CategoryDto>>;