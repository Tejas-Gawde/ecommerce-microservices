using EventContracts.Events;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Caching;

namespace OrdersService.Application.Consumers;

public sealed class InventoryReleasedConsumer
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<InventoryReleasedConsumer> _logger;

  public InventoryReleasedConsumer(
      IServiceProvider serviceProvider,
      ILogger<InventoryReleasedConsumer> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public async Task Consume(InventoryReleasedEvent message, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "Consuming InventoryReleased event for Product {ProductId}, Quantity: {Quantity}",
        message.ProductId,
        message.Quantity);

    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    var cacheService = scope.ServiceProvider.GetRequiredService<RedisCacheService>();

    try
    {
      // Find orders that contain this product and could be affected by release
      var orders = await dbContext.Orders
          .Include(o => o.OrderItems)
          .Where(o => o.Status == Domain.Enums.OrderStatus.InventoryReserved &&
                     o.OrderItems.Any(i => i.ProductId == message.ProductId))
          .ToListAsync(cancellationToken);

      foreach (var order in orders)
      {
        // In a real scenario, you'd check if the release affects this order
        // For now, we log and invalidate cache
        await cacheService.RemoveAsync($"order:{order.Id}");
      }

      await dbContext.SaveChangesAsync(cancellationToken);

      _logger.LogInformation(
          "Processed InventoryReleased event for Product {ProductId}, affected {OrderCount} orders",
          message.ProductId,
          orders.Count);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Error processing InventoryReleased event for Product {ProductId}",
          message.ProductId);
      throw;
    }
  }
}