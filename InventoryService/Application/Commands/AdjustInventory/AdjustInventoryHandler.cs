using EventContracts.Events;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Caching;
using InventoryService.Infrastructure.Messaging;

namespace InventoryService.Application.Commands.AdjustInventory;

public sealed class AdjustInventoryHandler : IRequestHandler<AdjustInventoryCommand>
{
  private readonly InventoryDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly RabbitMqPublisher _rabbitMqPublisher;
  private readonly ILogger<AdjustInventoryHandler> _logger;

  public AdjustInventoryHandler(
      InventoryDbContext dbContext,
      RedisCacheService cacheService,
      RabbitMqPublisher rabbitMqPublisher,
      ILogger<AdjustInventoryHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
  }

  public async Task Handle(AdjustInventoryCommand command, CancellationToken cancellationToken)
  {
    var inventoryItem = await _dbContext.InventoryItems
        .FirstOrDefaultAsync(i => i.ProductId == command.ProductId, cancellationToken);

    if (inventoryItem is null)
      throw new KeyNotFoundException($"Inventory not found for product ID: {command.ProductId}");

    var previousQuantity = inventoryItem.AvailableQuantity;
    inventoryItem.Adjust(command.NewAvailableQuantity);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // Invalidate cache
    await _cacheService.RemoveAsync($"inventory:product:{command.ProductId}");

    var inventoryAdjustedEvent = new InventoryAdjustedEvent
    {
      InventoryItemId = inventoryItem.Id,
      ProductId = inventoryItem.ProductId,
      PreviousAvailableQuantity = previousQuantity,
      NewAvailableQuantity = inventoryItem.AvailableQuantity,
      AdjustedAt = DateTime.UtcNow
    };

    await _rabbitMqPublisher.PublishAsync(inventoryAdjustedEvent);

    _logger.LogInformation(
        "Inventory adjusted for Product {ProductId}: {PreviousQuantity} -> {NewQuantity}",
        command.ProductId,
        previousQuantity,
        inventoryItem.AvailableQuantity);
  }
}