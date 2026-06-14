namespace EventContracts.Events;

public sealed record InventoryAdjustedEvent
{
  public Guid InventoryItemId { get; init; }
  public Guid ProductId { get; init; }
  public int PreviousAvailableQuantity { get; init; }
  public int NewAvailableQuantity { get; init; }
  public DateTime AdjustedAt { get; init; }
  public DateTime EventTimestamp { get; init; }
  public string EventType { get; init; } = nameof(InventoryAdjustedEvent);

  public InventoryAdjustedEvent()
  {
    EventTimestamp = DateTime.UtcNow;
  }
}