namespace EventContracts.Events;

public sealed record ProductCreatedEvent
{
  public Guid ProductId { get; init; }
  public required string Name { get; init; }
  public required string Description { get; init; }
  public decimal Price { get; init; }
  public int StockQuantity { get; init; }
  public Guid CategoryId { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime EventTimestamp { get; init; }
  public string EventType { get; init; } = nameof(ProductCreatedEvent);

  public ProductCreatedEvent()
  {
    EventTimestamp = DateTime.UtcNow;
  }
}