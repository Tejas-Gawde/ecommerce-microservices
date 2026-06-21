namespace OrdersService.Application.Dtos;

public sealed class OrderItemDto
{
  public Guid Id { get; init; }
  public Guid ProductId { get; init; }
  public int Quantity { get; init; }
  public decimal Price { get; init; }
  public decimal SubTotal { get; init; }
}