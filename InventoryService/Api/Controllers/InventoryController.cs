using InventoryService.Application.Commands.ReserveInventory;
using InventoryService.Application.Commands.ReleaseInventory;
using InventoryService.Application.Commands.AdjustInventory;
using InventoryService.Application.Queries.GetInventoryByProductId;
using InventoryService.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InventoryController : ControllerBase
{
  private readonly IMediator _mediator;

  public InventoryController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost("reserve")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ReserveInventory(
      [FromBody] ReserveInventoryCommand command,
      CancellationToken cancellationToken)
  {
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }

  [HttpPost("release")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ReleaseInventory(
      [FromBody] ReleaseInventoryCommand command,
      CancellationToken cancellationToken)
  {
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }

  [HttpPost("adjust")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> AdjustInventory(
      [FromBody] AdjustInventoryCommand command,
      CancellationToken cancellationToken)
  {
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }

  [HttpGet("{productId:guid}")]
  [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetInventoryByProductId(
      Guid productId,
      CancellationToken cancellationToken)
  {
    var query = new GetInventoryByProductIdQuery { ProductId = productId };
    var inventory = await _mediator.Send(query, cancellationToken);
    return Ok(inventory);
  }
}