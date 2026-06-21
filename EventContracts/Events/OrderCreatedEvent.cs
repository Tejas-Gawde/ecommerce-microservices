namespace EventContracts.Events;

public sealed record OrderCreatedEvent
{
  public Guid OrderId { get; init; }
  public Guid CustomerId { get; init; }
  public decimal TotalAmount { get; init; }
  public string Status { get; init; }
  public List<OrderItemDetail> Items { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime EventTimestamp { get; init; }
  public string EventType { get; init; } = nameof(OrderCreatedEvent);

  public OrderCreatedEvent()
  {
    EventTimestamp = DateTime.UtcNow;
  }
}

public sealed record OrderItemDetail
{
  public Guid ProductId { get; init; }
  public int Quantity { get; init; }
  public decimal Price { get; init; }
}