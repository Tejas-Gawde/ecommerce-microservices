using EasyNetQ;
using EventContracts.Events;
using InventoryService.Application.Consumers;

namespace InventoryService.Infrastructure.Messaging;

public sealed class RabbitMqSubscriber : IHostedService
{
  private readonly IBus _bus;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<RabbitMqSubscriber> _logger;
  private IDisposable? _productCreatedSubscription;

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

    // Subscribe to ProductCreated events from shared contracts
    _productCreatedSubscription = _bus.PubSub.SubscribeAsync<ProductCreatedEvent>(
        "inventory-service-product-created",
        async message =>
        {
          using var scope = _serviceProvider.CreateScope();
          var consumer = scope.ServiceProvider.GetRequiredService<ProductCreatedConsumer>();
          await consumer.Consume(message, cancellationToken);
        },
        config => config.WithQueueName("inventory.product.created")
                       .WithPrefetchCount(10)
                       .WithAutoDelete(false)
                       .WithDurable(true));

    _logger.LogInformation("RabbitMQ subscriptions started successfully");
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Stopping RabbitMQ subscriptions...");
    _productCreatedSubscription?.Dispose();
    _logger.LogInformation("RabbitMQ subscriptions stopped");
    return Task.CompletedTask;
  }
}