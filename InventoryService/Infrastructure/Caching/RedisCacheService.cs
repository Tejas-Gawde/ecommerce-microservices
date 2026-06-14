using StackExchange.Redis;
using System.Text.Json;

namespace InventoryService.Infrastructure.Caching;

public sealed class RedisCacheService
{
  private readonly IDatabase _database;
  private readonly IServer _server;
  private readonly ILogger<RedisCacheService> _logger;

  public RedisCacheService(
      IConnectionMultiplexer connectionMultiplexer,
      ILogger<RedisCacheService> logger)
  {
    _database = connectionMultiplexer.GetDatabase();
    _server = connectionMultiplexer.GetServer(
        connectionMultiplexer.GetEndPoints().First());
    _logger = logger;
  }

  public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
  {
    try
    {
      await _database.StringSetAsync(key, value, expiry);
      _logger.LogDebug("Cache set: {Key}", key);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error setting cache key: {Key}", key);
    }
  }

  public async Task<string?> GetAsync(string key)
  {
    try
    {
      var value = await _database.StringGetAsync(key);
      return value.HasValue ? value.ToString() : null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting cache key: {Key}", key);
      return null;
    }
  }

  public async Task RemoveAsync(string key)
  {
    try
    {
      await _database.KeyDeleteAsync(key);
      _logger.LogDebug("Cache removed: {Key}", key);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error removing cache key: {Key}", key);
    }
  }

  public async Task RemoveByPatternAsync(string pattern)
  {
    try
    {
      var keys = _server.Keys(pattern: pattern);
      foreach (var key in keys)
      {
        await _database.KeyDeleteAsync(key);
      }
      _logger.LogDebug("Cache pattern removed: {Pattern}", pattern);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error removing cache pattern: {Pattern}", pattern);
    }
  }

  public async Task<T?> GetOrSetAsync<T>(
      string key,
      Func<Task<T>> factory,
      TimeSpan? expiry = null) where T : class
  {
    var cached = await GetAsync(key);
    if (cached is not null)
    {
      return JsonSerializer.Deserialize<T>(cached);
    }

    var result = await factory();
    if (result is not null)
    {
      var serialized = JsonSerializer.Serialize(result);
      await SetAsync(key, serialized, expiry);
    }

    return result;
  }
}