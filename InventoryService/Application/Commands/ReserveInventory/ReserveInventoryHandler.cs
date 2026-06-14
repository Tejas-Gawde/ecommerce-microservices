using EventContracts.Events;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Caching;
using InventoryService.Infrastructure.Messaging;

namespace InventoryService.Application.Commands.ReserveInventory;

public sealed class ReserveInventoryHandler : IRequestHandler<ReserveInventoryCommand>
{
  private readonly InventoryDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly RabbitMqPublisher _rabbitMqPublisher;
  private readonly ILogger<ReserveInventoryHandler> _logger;

  public ReserveInventoryHandler(
      InventoryDbContext dbContext,
      RedisCacheService cacheService,
      RabbitMqPublisher rabbitMqPublisher,
      ILogger<ReserveInventoryHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
  }

  public async Task Handle(ReserveInventoryCommand command, CancellationToken cancellationToken)
  {
    var inventoryItem = await _dbContext.InventoryItems
        .FirstOrDefaultAsync(i => i.ProductId == command.ProductId, cancellationToken);

    if (inventoryItem is null)
      throw new KeyNotFoundException($"Inventory not found for product ID: {command.ProductId}");

    inventoryItem.Reserve(command.Quantity);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // Invalidate cache
    await _cacheService.RemoveAsync($"inventory:product:{command.ProductId}");

    var inventoryReservedEvent = new InventoryReservedEvent
    {
      InventoryItemId = inventoryItem.Id,
      ProductId = inventoryItem.ProductId,
      Quantity = command.Quantity,
      AvailableQuantity = inventoryItem.AvailableQuantity,
      ReservedQuantity = inventoryItem.ReservedQuantity,
      ReservedAt = DateTime.UtcNow
    };

    await _rabbitMqPublisher.PublishAsync(inventoryReservedEvent);

    _logger.LogInformation(
        "Inventory reserved for Product {ProductId}: {Quantity} units. Available: {Available}, Reserved: {Reserved}",
        command.ProductId,
        command.Quantity,
        inventoryItem.AvailableQuantity,
        inventoryItem.ReservedQuantity);
  }
}