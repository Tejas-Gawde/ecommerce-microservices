using EventContracts.Events;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Messaging;

namespace OrdersService.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
  private readonly OrdersDbContext _dbContext;
  private readonly RabbitMqPublisher _rabbitMqPublisher;
  private readonly ILogger<CreateOrderHandler> _logger;

  public CreateOrderHandler(
      OrdersDbContext dbContext,
      RabbitMqPublisher rabbitMqPublisher,
      ILogger<CreateOrderHandler> logger)
  {
    _dbContext = dbContext;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
  }

  public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
  {
    var orderItems = command.Items.Select(item =>
        (item.ProductId, item.Quantity, item.Price)).ToList();

    var order = Order.Create(command.CustomerId, orderItems);

    _dbContext.Orders.Add(order);
    await _dbContext.SaveChangesAsync(cancellationToken);

    // Publish OrderCreated event
    var orderCreatedEvent = new OrderCreatedEvent
    {
      OrderId = order.Id,
      CustomerId = order.CustomerId,
      TotalAmount = order.TotalAmount,
      Status = order.Status.ToString(),
      Items = order.OrderItems.Select(i => new OrderItemDetail
      {
        ProductId = i.ProductId,
        Quantity = i.Quantity,
        Price = i.Price
      }).ToList(),
      CreatedAt = order.CreatedAt
    };

    await _rabbitMqPublisher.PublishAsync(orderCreatedEvent);

    _logger.LogInformation(
        "Order created: {OrderId} for Customer {CustomerId}. Total: {TotalAmount}, Items: {ItemCount}",
        order.Id,
        order.CustomerId,
        order.TotalAmount,
        order.OrderItems.Count);

    return order.Id;
  }
}