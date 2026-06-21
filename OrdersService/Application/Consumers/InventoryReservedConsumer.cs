using EventContracts.Events;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Caching;

namespace OrdersService.Application.Consumers;

public sealed class InventoryReservedConsumer
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<InventoryReservedConsumer> _logger;

  public InventoryReservedConsumer(
      IServiceProvider serviceProvider,
      ILogger<InventoryReservedConsumer> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public async Task Consume(InventoryReservedEvent message, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "Consuming InventoryReserved event for Product {ProductId}, Quantity: {Quantity}",
        message.ProductId,
        message.Quantity);

    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    var cacheService = scope.ServiceProvider.GetRequiredService<RedisCacheService>();

    try
    {
      // Find orders that contain this product and are in Pending status
      var orders = await dbContext.Orders
          .Include(o => o.OrderItems)
          .Where(o => o.Status == Domain.Enums.OrderStatus.Pending &&
                     o.OrderItems.Any(i => i.ProductId == message.ProductId))
          .ToListAsync(cancellationToken);

      foreach (var order in orders)
      {
        // Check if all items in the order have inventory reserved
        var allItemsReserved = true; // Simplified logic - in production you'd track this per item

        if (allItemsReserved)
        {
          order.MarkInventoryReserved();

          // Invalidate cache for this order
          await cacheService.RemoveAsync($"order:{order.Id}");
        }
      }

      await dbContext.SaveChangesAsync(cancellationToken);

      _logger.LogInformation(
          "Updated {OrderCount} orders to InventoryReserved status for Product {ProductId}",
          orders.Count,
          message.ProductId);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Error processing InventoryReserved event for Product {ProductId}",
          message.ProductId);
      throw;
    }
  }
}