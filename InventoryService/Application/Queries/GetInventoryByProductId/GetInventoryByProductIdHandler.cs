using InventoryService.Application.Dtos;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Caching;
using System.Text.Json;

namespace InventoryService.Application.Queries.GetInventoryByProductId;

public sealed class GetInventoryByProductIdHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryDto>
{
  private readonly InventoryDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly ILogger<GetInventoryByProductIdHandler> _logger;

  private const int CacheDurationMinutes = 15;

  public GetInventoryByProductIdHandler(
      InventoryDbContext dbContext,
      RedisCacheService cacheService,
      ILogger<GetInventoryByProductIdHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _logger = logger;
  }

  public async Task<InventoryDto> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
  {
    var cacheKey = $"inventory:product:{request.ProductId}";

    // Check Redis cache first
    var cachedInventory = await _cacheService.GetAsync(cacheKey);
    if (cachedInventory is not null)
    {
      _logger.LogInformation("Cache hit for inventory product ID: {ProductId}", request.ProductId);
      var deserializedInventory = JsonSerializer.Deserialize<InventoryDto>(cachedInventory);
      if (deserializedInventory is not null)
        return deserializedInventory;
    }

    _logger.LogInformation("Cache miss for inventory product ID: {ProductId}", request.ProductId);

    var inventoryItem = await _dbContext.InventoryItems
        .AsNoTracking()
        .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

    if (inventoryItem is null)
      throw new KeyNotFoundException($"Inventory not found for product ID: {request.ProductId}");

    var inventoryDto = new InventoryDto
    {
      Id = inventoryItem.Id,
      ProductId = inventoryItem.ProductId,
      AvailableQuantity = inventoryItem.AvailableQuantity,
      ReservedQuantity = inventoryItem.ReservedQuantity,
      TotalQuantity = inventoryItem.TotalQuantity,
      LastUpdated = inventoryItem.LastUpdated
    };

    // Cache the result
    var serializedInventory = JsonSerializer.Serialize(inventoryDto);
    await _cacheService.SetAsync(cacheKey, serializedInventory, TimeSpan.FromMinutes(CacheDurationMinutes));

    return inventoryDto;
  }
}