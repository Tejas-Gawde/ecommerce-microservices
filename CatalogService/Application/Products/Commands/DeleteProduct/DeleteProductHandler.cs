using EventContracts.Events;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Caching;
using CatalogService.Infrastructure.Messaging;

namespace CatalogService.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
{
  private readonly CatalogDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly RabbitMqPublisher _rabbitMqPublisher;
  private readonly ILogger<DeleteProductHandler> _logger;

  public DeleteProductHandler(
      CatalogDbContext dbContext,
      RedisCacheService cacheService,
      RabbitMqPublisher rabbitMqPublisher,
      ILogger<DeleteProductHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
  }

  public async Task Handle(DeleteProductCommand command, CancellationToken cancellationToken)
  {
    var product = await _dbContext.Products
        .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

    if (product is null)
      throw new KeyNotFoundException($"Product with ID {command.Id} not found.");

    _dbContext.Products.Remove(product);
    await _dbContext.SaveChangesAsync(cancellationToken);

    await _cacheService.RemoveAsync($"product:{command.Id}");
    await _cacheService.RemoveByPatternAsync("products:*");

    var productDeletedEvent = new ProductDeletedEvent
    {
      ProductId = command.Id,
      DeletedAt = DateTime.UtcNow
    };

    await _rabbitMqPublisher.PublishAsync(productDeletedEvent);

    _logger.LogInformation(
        "Product deleted: {ProductId}",
        command.Id);
  }
}