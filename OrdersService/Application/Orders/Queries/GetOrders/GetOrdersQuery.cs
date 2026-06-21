using OrdersService.Application.Dtos;

namespace OrdersService.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery : IRequest<List<OrderDto>>
{
  public Guid? CustomerId { get; init; }
  public string? Status { get; init; }
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 10;
}