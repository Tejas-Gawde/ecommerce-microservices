namespace InventoryService.Application.Commands.AdjustInventory;

public sealed record AdjustInventoryCommand : IRequest
{
  public Guid ProductId { get; init; }
  public int NewAvailableQuantity { get; init; }
}