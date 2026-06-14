using CatalogService.Domain.Entities;
using EventContracts.Events;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Caching;
using CatalogService.Infrastructure.Messaging;

namespace CatalogService.Application.Products.Commands.CreateProduct;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
  private readonly CatalogDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly RabbitMqPublisher _rabbitMqPublisher;
  private readonly ILogger<CreateProductHandler> _logger;

  public CreateProductHandler(
      CatalogDbContext dbContext,
      RedisCacheService cacheService,
      RabbitMqPublisher rabbitMqPublisher,
      ILogger<CreateProductHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _rabbitMqPublisher = rabbitMqPublisher;
    _logger = logger;
  }

  public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
  {
    var categoryExists = await _dbContext.Categories
        .AnyAsync(c => c.Id == command.CategoryId, cancellationToken);

    if (!categoryExists)
      throw new InvalidOperationException($"Category with ID {command.CategoryId} not found.");

    var product = Product.Create(
        command.Name,
        command.Description,
        command.Price,
        command.StockQuantity,
        command.CategoryId);

    _dbContext.Products.Add(product);
    await _dbContext.SaveChangesAsync(cancellationToken);

    var productCreatedEvent = new ProductCreatedEvent
    {
      ProductId = product.Id,
      Name = product.Name,
      Description = product.Description,
      Price = product.Price,
      StockQuantity = product.StockQuantity,
      CategoryId = product.CategoryId,
      CreatedAt = product.CreatedAt
    };

    await _rabbitMqPublisher.PublishAsync(productCreatedEvent);

    _logger.LogInformation(
        "Product created: {ProductId} - {ProductName}",
        product.Id,
        product.Name);

    return product.Id;
  }
}