using OrdersService.Application.Dtos;

namespace OrdersService.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery : IRequest<OrderDto>
{
  public Guid OrderId { get; init; }
}