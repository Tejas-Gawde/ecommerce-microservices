using EasyNetQ;
using EventContracts.Events;
using OrdersService.Application.Consumers;

namespace OrdersService.Infrastructure.Messaging;

public sealed class RabbitMqSubscriber : IHostedService
{
  private readonly IBus _bus;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<RabbitMqSubscriber> _logger;
  private IDisposable? _inventoryReservedSubscription;
  private IDisposable? _inventoryReleasedSubscription;

  public RabbitMqSubscriber(
      IBus bus,
      IServiceProvider serviceProvider,
      ILogger<RabbitMqSubscriber> logger)
  {
    _bus = bus;
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting RabbitMQ subscriptions...");

    // Subscribe to InventoryReserved events
    _inventoryReservedSubscription = _bus.PubSub.SubscribeAsync<InventoryReservedEvent>(
        "orders-service-inventory-reserved",
        async message =>
        {
          using var scope = _serviceProvider.CreateScope();
          var consumer = scope.ServiceProvider.GetRequiredService<InventoryReservedConsumer>();
          await consumer.Consume(message, cancellationToken);
        },
        config => config.WithQueueName("orders.inventory.reserved")
                       .WithPrefetchCount(10)
                       .WithAutoDelete(false)
                       .WithDurable(true));

    // Subscribe to InventoryReleased events
    _inventoryReleasedSubscription = _bus.PubSub.SubscribeAsync<InventoryReleasedEvent>(
        "orders-service-inventory-released",
        async message =>
        {
          using var scope = _serviceProvider.CreateScope();
          var consumer = scope.ServiceProvider.GetRequiredService<InventoryReleasedConsumer>();
          await consumer.Consume(message, cancellationToken);
        },
        config => config.WithQueueName("orders.inventory.released")
                       .WithPrefetchCount(10)
                       .WithAutoDelete(false)
                       .WithDurable(true));

    _logger.LogInformation("RabbitMQ subscriptions started successfully");
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Stopping RabbitMQ subscriptions...");
    _inventoryReservedSubscription?.Dispose();
    _inventoryReleasedSubscription?.Dispose();
    _logger.LogInformation("RabbitMQ subscriptions stopped");
    return Task.CompletedTask;
  }
}