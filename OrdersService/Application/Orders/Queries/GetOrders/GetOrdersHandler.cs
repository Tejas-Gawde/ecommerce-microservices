using OrdersService.Application.Dtos;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Caching;
using System.Text.Json;

namespace OrdersService.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersHandler : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
  private readonly OrdersDbContext _dbContext;
  private readonly RedisCacheService _cacheService;
  private readonly ILogger<GetOrdersHandler> _logger;

  private const int CacheDurationMinutes = 10;

  public GetOrdersHandler(
      OrdersDbContext dbContext,
      RedisCacheService cacheService,
      ILogger<GetOrdersHandler> logger)
  {
    _dbContext = dbContext;
    _cacheService = cacheService;
    _logger = logger;
  }

  public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
  {
    var cacheKey = $"orders:customer={request.CustomerId}:status={request.Status}:page={request.Page}:size={request.PageSize}";

    // Check Redis cache first
    var cachedOrders = await _cacheService.GetAsync(cacheKey);
    if (cachedOrders is not null)
    {
      _logger.LogInformation("Cache hit for orders query: {CacheKey}", cacheKey);
      return JsonSerializer.Deserialize<List<OrderDto>>(cachedOrders);
    }

    _logger.LogInformation("Cache miss for orders query: {CacheKey}", cacheKey);

    IQueryable<Domain.Entities.Order> query = _dbContext.Orders
        .Include(o => o.OrderItems)
        .AsNoTracking();

    // Apply filters
    if (request.CustomerId.HasValue)
    {
      query = query.Where(o => o.CustomerId == request.CustomerId.Value);
    }

    if (!string.IsNullOrWhiteSpace(request.Status) &&
        Enum.TryParse<OrderStatus>(request.Status, true, out var status))
    {
      query = query.Where(o => o.Status == status);
    }

    // Apply ordering and pagination
    var orders = await query
        .OrderByDescending(o => o.CreatedAt)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);

    var orderDtos = orders.Select(MapToDto).ToList();

    // Cache the result
    var serializedOrders = JsonSerializer.Serialize(orderDtos);
    await _cacheService.SetAsync(cacheKey, serializedOrders, TimeSpan.FromMinutes(CacheDurationMinutes));

    return orderDtos;
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