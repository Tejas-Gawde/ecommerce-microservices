using OrdersService.Application.Orders.Commands.CreateOrder;
using OrdersService.Application.Orders.Commands.CancelOrder;
using OrdersService.Application.Orders.Queries.GetOrderById;
using OrdersService.Application.Orders.Queries.GetOrders;
using OrdersService.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
  private readonly IMediator _mediator;

  public OrdersController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateOrder(
      [FromBody] CreateOrderCommand command,
      CancellationToken cancellationToken)
  {
    var orderId = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(
        nameof(GetOrderById),
        new { orderId },
        orderId);
  }

  [HttpPost("{orderId:guid}/cancel")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CancelOrder(
      Guid orderId,
      [FromBody] string reason = "Order cancelled",
      CancellationToken cancellationToken = default)
  {
    var command = new CancelOrderCommand
    {
      OrderId = orderId,
      Reason = reason
    };
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }

  [HttpGet("{orderId:guid}")]
  [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetOrderById(
      Guid orderId,
      CancellationToken cancellationToken)
  {
    var query = new GetOrderByIdQuery { OrderId = orderId };
    var order = await _mediator.Send(query, cancellationToken);
    return Ok(order);
  }

  [HttpGet]
  [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetOrders(
      [FromQuery] Guid? customerId = null,
      [FromQuery] string? status = null,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      CancellationToken cancellationToken = default)
  {
    var query = new GetOrdersQuery
    {
      CustomerId = customerId,
      Status = status,
      Page = page,
      PageSize = pageSize
    };

    var orders = await _mediator.Send(query, cancellationToken);
    return Ok(orders);
  }
}