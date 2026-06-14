namespace InventoryService.Application.Commands.ReleaseInventory;

public sealed record ReleaseInventoryCommand : IRequest
{
  public Guid ProductId { get; init; }
  public int Quantity { get; init; }
}