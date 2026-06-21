namespace OrdersService.Application.Dtos;

public sealed class OrderDto
{
  public Guid Id { get; init; }
  public Guid CustomerId { get; init; }
  public required string Status { get; init; }
  public decimal TotalAmount { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime? UpdatedAt { get; init; }
  public List<OrderItemDto> Items { get; init; }
}