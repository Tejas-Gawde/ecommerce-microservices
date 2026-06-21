namespace OrdersService.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand : IRequest<Guid>
{
  public Guid CustomerId { get; init; }
  public List<OrderItemRequest> Items { get; init; }
}

public sealed record OrderItemRequest
{
  public Guid ProductId { get; init; }
  public int Quantity { get; init; }
  public decimal Price { get; init; }
}