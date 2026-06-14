using CatalogService.Application.Categories.Commands.CreateCategory;
using CatalogService.Application.Categories.Commands.UpdateCategory;
using CatalogService.Application.Categories.Commands.DeleteCategory;
using CatalogService.Application.Categories.Queries.GetCategoryById;
using CatalogService.Application.Categories.Queries.GetCategories;
using CatalogService.Application.Categories.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
  private readonly IMediator _mediator;

  public CategoriesController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
  public async Task<IActionResult> CreateCategory(
      [FromBody] CreateCategoryCommand command,
      CancellationToken cancellationToken)
  {
    var categoryId = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(
        nameof(GetCategoryById),
        new { id = categoryId },
        categoryId);
  }

  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> UpdateCategory(
      Guid id,
      [FromBody] UpdateCategoryCommand command,
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
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> DeleteCategory(
      Guid id,
      CancellationToken cancellationToken)
  {
    var command = new DeleteCategoryCommand { Id = id };
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }

  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetCategoryById(
      Guid id,
      CancellationToken cancellationToken)
  {
    var query = new GetCategoryByIdQuery { Id = id };
    var category = await _mediator.Send(query, cancellationToken);
    return Ok(category);
  }

  [HttpGet]
  [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetCategories(
      CancellationToken cancellationToken)
  {
    var query = new GetCategoriesQuery();
    var categories = await _mediator.Send(query, cancellationToken);
    return Ok(categories);
  }
}