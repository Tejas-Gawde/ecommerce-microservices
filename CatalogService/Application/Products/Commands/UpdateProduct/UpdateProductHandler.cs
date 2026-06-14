using EventContracts.Events;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Caching;
using CatalogService.Infrastructure.Messaging;

namespace CatalogService.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
{
  private readonly CatalogDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly RabbitMqPublisher _rabbitMqPublisher;
  private readonly ILogger<UpdateProductHandler> _logger;

  public UpdateProductHandler(
      CatalogDbContext dbContext,
      RedisCacheService cacheService,
      RabbitMqPublisher rabbitMqPublisher,
      ILogger<UpdateProductHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
  }

  public async Task Handle(UpdateProductCommand command, CancellationToken cancellationToken)
  {
    var product = await _dbContext.Products
        .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

    if (product is null)
      throw new KeyNotFoundException($"Product with ID {command.Id} not found.");

    var categoryExists = await _dbContext.Categories
        .AnyAsync(c => c.Id == command.CategoryId, cancellationToken);

    if (!categoryExists)
      throw new InvalidOperationException($"Category with ID {command.CategoryId} not found.");

    product.Update(
        command.Name,
        command.Description,
        command.Price,
        command.StockQuantity,
        command.CategoryId);

    await _dbContext.SaveChangesAsync(cancellationToken);

    // Invalidate cache
    await _cacheService.RemoveAsync($"product:{command.Id}");
    await _cacheService.RemoveByPatternAsync("products:*");

    var productUpdatedEvent = new ProductUpdatedEvent
    {
      ProductId = product.Id,
      Name = product.Name,
      Description = product.Description,
      Price = product.Price,
      StockQuantity = product.StockQuantity,
      CategoryId = product.CategoryId,
    };

    await _rabbitMqPublisher.PublishAsync(productUpdatedEvent);

    _logger.LogInformation(
        "Product updated: {ProductId} - {ProductName}",
        product.Id,
        product.Name);
  }
}