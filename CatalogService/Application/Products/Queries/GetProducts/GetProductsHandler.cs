using CatalogService.Application.Products.Dtos;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Caching;
using System.Text.Json;

namespace CatalogService.Application.Products.Queries.GetProducts;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
  private readonly CatalogDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly ILogger<GetProductsHandler> _logger;

  private const int CacheDurationMinutes = 15;

  public GetProductsHandler(
      CatalogDbContext dbContext,
      RedisCacheService cacheService,
      ILogger<GetProductsHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _logger = logger;
  }

  public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
  {
    var cacheKey = $"products:page={request.Page}:size={request.PageSize}:search={request.SearchTerm}:category={request.CategoryId}:sort={request.SortBy}:desc={request.SortDescending}";

    // Check Redis cache first
    var cachedData = await _cacheService.GetAsync(cacheKey);
    if (cachedData is not null)
    {
      _logger.LogInformation("Cache hit for products query: {CacheKey}", cacheKey);
      var deserializedData = JsonSerializer.Deserialize<List<ProductDto>>(cachedData);
      if (deserializedData is not null)
        return deserializedData;
    }

    _logger.LogInformation("Cache miss for products query: {CacheKey}", cacheKey);

    IQueryable<Domain.Entities.Product> query = _dbContext.Products
        .Include(p => p.Category)
        .AsNoTracking();

    // Apply filtering
    if (!string.IsNullOrWhiteSpace(request.SearchTerm))
    {
      var searchTerm = request.SearchTerm.ToLower();
      query = query.Where(p =>
          p.Name.ToLower().Contains(searchTerm) ||
          p.Description.ToLower().Contains(searchTerm));
    }

    if (request.CategoryId.HasValue)
    {
      query = query.Where(p => p.CategoryId == request.CategoryId.Value);
    }

    // Apply sorting
    query = request.SortBy?.ToLower() switch
    {
      "name" => request.SortDescending
          ? query.OrderByDescending(p => p.Name)
          : query.OrderBy(p => p.Name),
      "price" => request.SortDescending
          ? query.OrderByDescending(p => p.Price)
          : query.OrderBy(p => p.Price),
      "createdat" => request.SortDescending
          ? query.OrderByDescending(p => p.CreatedAt)
          : query.OrderBy(p => p.CreatedAt),
      _ => query.OrderBy(p => p.Name)
    };

    // Apply pagination
    var products = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);

    var productDtos = products.Select(p => new ProductDto
    {
      Id = p.Id,
      Name = p.Name,
      Description = p.Description,
      Price = p.Price,
      StockQuantity = p.StockQuantity,
      CategoryId = p.CategoryId,
      CategoryName = p.Category.Name,
      CreatedAt = p.CreatedAt,
      UpdatedAt = p.UpdatedAt
    }).ToList();

    // Cache the result
    var serializedData = JsonSerializer.Serialize(productDtos);
    await _cacheService.SetAsync(cacheKey, serializedData, TimeSpan.FromMinutes(CacheDurationMinutes));

    return productDtos;
  }
}