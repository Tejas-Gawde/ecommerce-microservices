namespace EventContracts.Events;

public sealed record ProductDeletedEvent
{
  public Guid ProductId { get; init; }
  public DateTime DeletedAt { get; init; }
  public DateTime EventTimestamp { get; init; }
  public string EventType { get; init; } = nameof(ProductDeletedEvent);

  public ProductDeletedEvent()
  {
    EventTimestamp = DateTime.UtcNow;
  }
}