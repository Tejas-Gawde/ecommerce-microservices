using EventContracts.Events;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Caching;

namespace InventoryService.Application.Consumers;

public sealed class ProductCreatedConsumer
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ProductCreatedConsumer> _logger;

  public ProductCreatedConsumer(
      IServiceProvider serviceProvider,
      ILogger<ProductCreatedConsumer> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public async Task Consume(ProductCreatedEvent message, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "Consuming ProductCreated event for Product {ProductId}: {ProductName} with stock quantity {StockQuantity}",
        message.ProductId,
        message.Name,
        message.StockQuantity);

    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var cacheService = scope.ServiceProvider.GetRequiredService<RedisCacheService>();

    try
    {
      // Check if inventory already exists for this product (idempotency)
      var existingInventory = await dbContext.InventoryItems
          .FirstOrDefaultAsync(i => i.ProductId == message.ProductId, cancellationToken);

      if (existingInventory is not null)
      {
        _logger.LogWarning(
            "Inventory already exists for Product {ProductId}. Skipping creation.",
            message.ProductId);
        return;
      }

      var inventoryItem = InventoryItem.Create(message.ProductId, message.StockQuantity);

      dbContext.InventoryItems.Add(inventoryItem);
      await dbContext.SaveChangesAsync(cancellationToken);

      // Invalidate any potential cache
      await cacheService.RemoveAsync($"inventory:product:{message.ProductId}");

      _logger.LogInformation(
          "Inventory created for Product {ProductId} with {Quantity} units",
          message.ProductId,
          inventoryItem.AvailableQuantity);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Error processing ProductCreated event for Product {ProductId}",
          message.ProductId);
      throw; // Re-throw to trigger NACK and retry
    }
  }
}