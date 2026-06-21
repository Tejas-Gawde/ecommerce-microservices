using OrdersService.Application.Dtos;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Caching;
using System.Text.Json;

namespace OrdersService.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
  private readonly OrdersDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly ILogger<GetOrderByIdHandler> _logger;

  private const int CacheDurationMinutes = 15;

  public GetOrderByIdHandler(
      OrdersDbContext dbContext,
      RedisCacheService cacheService,
      ILogger<GetOrderByIdHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _logger = logger;
  }

  public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
  {
    var cacheKey = $"order:{request.OrderId}";

    // Check Redis cache first
    var cachedOrder = await _cacheService.GetAsync(cacheKey);
    if (cachedOrder is not null)
    {
      _logger.LogInformation("Cache hit for order ID: {OrderId}", request.OrderId);
      return JsonSerializer.Deserialize<OrderDto>(cachedOrder);
    }

    _logger.LogInformation("Cache miss for order ID: {OrderId}", request.OrderId);

    var order = await _dbContext.Orders
        .Include(o => o.OrderItems)
        .AsNoTracking()
        .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

    if (order is null)
      throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");

    var orderDto = MapToDto(order);

    // Cache the result
    var serializedOrder = JsonSerializer.Serialize(orderDto);
    await _cacheService.SetAsync(cacheKey, serializedOrder, TimeSpan.FromMinutes(CacheDurationMinutes));

    return orderDto;
  }

  private static OrderDto MapToDto(Domain.Entities.Order order)
  {
    return new OrderDto
    {
      Id = order.Id,
      CustomerId = order.CustomerId,
      Status = order.Status.ToString(),
      TotalAmount = order.TotalAmount,
      CreatedAt = order.CreatedAt,
      UpdatedAt = order.UpdatedAt,
      Items = order.OrderItems.Select(item => new OrderItemDto
      {
        Id = item.Id,
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        Price = item.Price,
        SubTotal = item.Price * item.Quantity
      }).ToList()
    };
  }
}