namespace InventoryService.Application.Dtos;

public sealed class InventoryDto
{
  public Guid Id { get; init; }
  public Guid ProductId { get; init; }
  public int AvailableQuantity { get; init; }
  public int ReservedQuantity { get; init; }
  public int TotalQuantity { get; init; }
  public DateTime LastUpdated { get; init; }
}