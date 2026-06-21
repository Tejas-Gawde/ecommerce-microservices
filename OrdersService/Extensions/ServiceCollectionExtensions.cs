using OrdersService.Infrastructure.Persistence;
using OrdersService.Infrastructure.Caching;
using OrdersService.Infrastructure.Messaging;
using OrdersService.Application.Consumers;
using EasyNetQ;
using StackExchange.Redis;
using Serilog;
using Polly;
using Polly.Extensions.Http;

namespace OrdersService.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddOrdersServices(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // MediatR
    services.AddMediatR(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    });

    // PostgreSQL
    services.AddDbContext<OrdersDbContext>(options =>
        options.UseNpgsql(
            configuration.GetConnectionString("OrdersDb"),
            npgsqlOptions =>
            {
              npgsqlOptions.EnableRetryOnFailure(
                      maxRetryCount: 3,
                      maxRetryDelay: TimeSpan.FromSeconds(10),
                      errorCodesToAdd: null);
            }));

    // Redis
    var redisConnectionString = configuration.GetConnectionString("Redis");
    services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
      var configurationOptions = ConfigurationOptions.Parse(redisConnectionString!);
      configurationOptions.AbortOnConnectFail = false;
      configurationOptions.ConnectTimeout = 5000;
      configurationOptions.SyncTimeout = 5000;
      return ConnectionMultiplexer.Connect(configurationOptions);
    });
    services.AddScoped<RedisCacheService>();

    // RabbitMQ with EasyNetQ v8
    var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");
    services.AddEasyNetQ(rabbitMqConnectionString!)
        .UseSystemTextJson();

    services.AddScoped<RabbitMqPublisher>();

    // Register consumers
    services.AddScoped<InventoryReservedConsumer>();
    services.AddScoped<InventoryReleasedConsumer>();

    // Register subscriber as hosted service
    services.AddHostedService<RabbitMqSubscriber>();

    // HTTP Client with Polly resilience
    services.AddHttpClient("OrdersClient")
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

    return services;
  }

  private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
  {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
              Log.Warning(
                      "Retry {RetryAttempt} after {Delay}ms due to {StatusCode}",
                      retryAttempt,
                      timespan.TotalMilliseconds,
                      outcome.Result?.StatusCode);
            });
  }

  private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
  {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            5,
            TimeSpan.FromSeconds(30),
            onBreak: (outcome, breakDelay) =>
            {
              Log.Error(
                      "Circuit breaker opened for {BreakDelay}ms due to {StatusCode}",
                      breakDelay.TotalMilliseconds,
                      outcome.Result?.StatusCode);
            },
            onReset: () =>
            {
              Log.Information("Circuit breaker reset");
            });
  }
}