using CatalogService.Application.Products.Dtos;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Caching;
using System.Text.Json;

namespace CatalogService.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
  private readonly CatalogDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly ILogger<GetProductByIdHandler> _logger;

  private const int CacheDurationMinutes = 15;

  public GetProductByIdHandler(
      CatalogDbContext dbContext,
      RedisCacheService cacheService,
      ILogger<GetProductByIdHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _logger = logger;
  }

  public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
  {
    var cacheKey = $"product:{request.Id}";

    // Check Redis cache first
    var cachedProduct = await _cacheService.GetAsync(cacheKey);
    if (cachedProduct is not null)
    {
      _logger.LogInformation("Cache hit for product ID: {ProductId}", request.Id);
      var deserializedProduct = JsonSerializer.Deserialize<ProductDto>(cachedProduct);
      if (deserializedProduct is not null)
        return deserializedProduct;
    }

    _logger.LogInformation("Cache miss for product ID: {ProductId}", request.Id);

    var product = await _dbContext.Products
        .Include(p => p.Category)
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

    if (product is null)
      throw new KeyNotFoundException($"Product with ID {request.Id} not found.");

    var productDto = new ProductDto
    {
      Id = product.Id,
      Name = product.Name,
      Description = product.Description,
      Price = product.Price,
      StockQuantity = product.StockQuantity,
      CategoryId = product.CategoryId,
      CategoryName = product.Category.Name,
      CreatedAt = product.CreatedAt,
      UpdatedAt = product.UpdatedAt
    };

    // Cache the result
    var serializedProduct = JsonSerializer.Serialize(productDto);
    await _cacheService.SetAsync(cacheKey, serializedProduct, TimeSpan.FromMinutes(CacheDurationMinutes));

    return productDto;
  }
}