namespace EventContracts.Events;

public sealed record OrderCancelledEvent
{
  public Guid OrderId { get; init; }
  public Guid CustomerId { get; init; }
  public string Status { get; init; }
  public decimal TotalAmount { get; init; }
  public string Reason { get; init; }
  public DateTime CancelledAt { get; init; }
  public List<OrderItemDetail> Items { get; init; }
  public DateTime EventTimestamp { get; init; }
  public string EventType { get; init; } = nameof(OrderCancelledEvent);

  public OrderCancelledEvent()
  {
    EventTimestamp = DateTime.UtcNow;
  }
}