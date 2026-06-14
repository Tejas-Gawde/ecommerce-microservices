using InventoryService.Application.Dtos;

namespace InventoryService.Application.Queries.GetInventoryByProductId;

public sealed record GetInventoryByProductIdQuery : IRequest<InventoryDto>
{
  public Guid ProductId { get; init; }
}