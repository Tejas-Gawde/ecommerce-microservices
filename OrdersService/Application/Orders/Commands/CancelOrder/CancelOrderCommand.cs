namespace OrdersService.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand : IRequest
{
  public Guid OrderId { get; init; }
  public string Reason { get; init; } = "Order cancelled";
}