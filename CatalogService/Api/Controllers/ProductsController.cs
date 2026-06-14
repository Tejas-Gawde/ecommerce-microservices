using CatalogService.Application.Products.Commands.CreateProduct;
using CatalogService.Application.Products.Commands.UpdateProduct;
using CatalogService.Application.Products.Commands.DeleteProduct;
using CatalogService.Application.Products.Queries.GetProductById;
using CatalogService.Application.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Mvc;
using CatalogService.Application.Products.Dtos;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
  private readonly IMediator _mediator;

  public ProductsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateProduct(
      [FromBody] CreateProductCommand command,
      CancellationToken cancellationToken)
  {
    var productId = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(
        nameof(GetProductById),
        new { id = productId },
        productId);
  }

  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> UpdateProduct(
      Guid id,
      [FromBody] UpdateProductCommand command,
      CancellationToken cancellationToken)
  {
    if (id != command.Id)
      return BadRequest("ID mismatch");

    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteProduct(
      Guid id,
      CancellationToken cancellationToken)
  {
    var command = new DeleteProductCommand { Id = id };
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }

  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetProductById(
      Guid id,
      CancellationToken cancellationToken)
  {
    var query = new GetProductByIdQuery { Id = id };
    var product = await _mediator.Send(query, cancellationToken);
    return Ok(product);
  }

  [HttpGet]
  [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetProducts(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] string? searchTerm = null,
      [FromQuery] Guid? categoryId = null,
      [FromQuery] string? sortBy = null,
      [FromQuery] bool sortDescending = false,
      CancellationToken cancellationToken = default)
  {
    var query = new GetProductsQuery
    {
      Page = page,
      PageSize = pageSize,
      SearchTerm = searchTerm,
      CategoryId = categoryId,
      SortBy = sortBy,
      SortDescending = sortDescending
    };

    var products = await _mediator.Send(query, cancellationToken);
    return Ok(products);
  }
}