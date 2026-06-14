using EventContracts.Events;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Caching;
using InventoryService.Infrastructure.Messaging;

namespace InventoryService.Application.Commands.ReleaseInventory;

public sealed class ReleaseInventoryHandler : IRequestHandler<ReleaseInventoryCommand>
{
  private readonly InventoryDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly RabbitMqPublisher _rabbitMqPublisher;
  private readonly ILogger<ReleaseInventoryHandler> _logger;

  public ReleaseInventoryHandler(
      InventoryDbContext dbContext,
      RedisCacheService cacheService,
      RabbitMqPublisher rabbitMqPublisher,
      ILogger<ReleaseInventoryHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
  }

  public async Task Handle(ReleaseInventoryCommand command, CancellationToken cancellationToken)
  {
    var inventoryItem = await _dbContext.InventoryItems
        .FirstOrDefaultAsync(i => i.ProductId == command.ProductId, cancellationToken);

    if (inventoryItem is null)
      throw new KeyNotFoundException($"Inventory not found for product ID: {command.ProductId}");

    inventoryItem.Release(command.Quantity);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // Invalidate cache
    await _cacheService.RemoveAsync($"inventory:product:{command.ProductId}");

    var inventoryReleasedEvent = new InventoryReleasedEvent
    {
      InventoryItemId = inventoryItem.Id,
      ProductId = inventoryItem.ProductId,
      Quantity = command.Quantity,
      AvailableQuantity = inventoryItem.AvailableQuantity,
      ReservedQuantity = inventoryItem.ReservedQuantity,
      ReleasedAt = DateTime.UtcNow
    };

    await _rabbitMqPublisher.PublishAsync(inventoryReleasedEvent);

    _logger.LogInformation(
        "Inventory released for Product {ProductId}: {Quantity} units. Available: {Available}, Reserved: {Reserved}",
        command.ProductId,
        command.Quantity,
        inventoryItem.AvailableQuantity,
        inventoryItem.ReservedQuantity);
  }
}