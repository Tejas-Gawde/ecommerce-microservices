using EventContracts.Events;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Messaging;

namespace OrdersService.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly OrdersDbContext _dbContext;
    private readonly RabbitMqPublisher _rabbitMqPublisher;
    private readonly ILogger<CancelOrderHandler> _logger;

    public CancelOrderHandler(
        OrdersDbContext dbContext,
        RabbitMqPublisher rabbitMqPublisher,
        ILogger<CancelOrderHandler> logger)
    {
        _dbContext = dbContext;
        _rabbitMqPublisher = rabbitMqPublisher;
        _logger = logger;
    }

    public async Task Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException($"Order with ID {command.OrderId} not found.");

        order.Cancel(command.Reason);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Publish OrderCancelled event
        var orderCancelledEvent = new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Reason = command.Reason,
            CancelledAt = DateTime.UtcNow,
            Items = order.OrderItems.Select(i => new OrderItemDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        await _rabbitMqPublisher.PublishAsync(orderCancelledEvent);

        _logger.LogInformation(
            "Order cancelled: {OrderId}. Reason: {Reason}",
            command.OrderId,
            command.Reason);
    }
}