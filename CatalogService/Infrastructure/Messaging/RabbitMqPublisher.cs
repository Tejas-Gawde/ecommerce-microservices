using EasyNetQ;

namespace CatalogService.Infrastructure.Messaging;

public class RabbitMqPublisher
{
  private readonly IBus _bus;
  private readonly ILogger<RabbitMqPublisher> _logger;

  public RabbitMqPublisher(IBus bus, ILogger<RabbitMqPublisher> logger)
  {
    _bus = bus;
    _logger = logger;
  }

  public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
  {
    try
    {
      await _bus.PubSub.PublishAsync(message, cancellationToken);
      _logger.LogInformation("Successfully published message of type {MessageType}", typeof(T).Name);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error publishing message of type {MessageType}", typeof(T).Name);
      throw;
    }
  }

  public async Task PublishWithDelayAsync<T>(T message, TimeSpan delay, CancellationToken cancellationToken = default) where T : class
  {
    try
    {
      await _bus.Scheduler.FuturePublishAsync(message, delay, cancellationToken);
      _logger.LogInformation("Successfully scheduled message of type {MessageType} with delay {Delay}",
          typeof(T).Name, delay);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error scheduling message of type {MessageType}", typeof(T).Name);
      throw;
    }
  }
}