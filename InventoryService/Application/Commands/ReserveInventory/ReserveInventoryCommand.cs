namespace InventoryService.Application.Commands.ReserveInventory;

public sealed record ReserveInventoryCommand : IRequest
{
  public Guid ProductId { get; init; }
  public int Quantity { get; init; }
}