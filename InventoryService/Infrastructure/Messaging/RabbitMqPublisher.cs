using EasyNetQ;

namespace InventoryService.Infrastructure.Messaging;

public sealed class RabbitMqPublisher
{
  private readonly IBus _bus;
  private readonly ILogger<RabbitMqPublisher> _logger;

  public RabbitMqPublisher(IBus bus, ILogger<RabbitMqPublisher> logger)
  {
    _bus = bus;
    _logger = logger;
  }

  public async Task PublishAsync<T>(T message) where T : class
  {
    try
    {
      await _bus.PubSub.PublishAsync(message);
      _logger.LogInformation(
          "Published event {EventType} with data: {@EventData}",
          typeof(T).Name,
          message);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to publish event {EventType}",
          typeof(T).Name);
      throw;
    }
  }
}