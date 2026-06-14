namespace EventContracts.Events;

public sealed record InventoryReservedEvent
{
  public Guid InventoryItemId { get; init; }
  public Guid ProductId { get; init; }
  public int Quantity { get; init; }
  public int AvailableQuantity { get; init; }
  public int ReservedQuantity { get; init; }
  public DateTime ReservedAt { get; init; }
  public DateTime EventTimestamp { get; init; }
  public string EventType { get; init; } = nameof(InventoryReservedEvent);

  public InventoryReservedEvent()
  {
    EventTimestamp = DateTime.UtcNow;
  }
}