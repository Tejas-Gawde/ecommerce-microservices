using OrdersService.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
		configuration.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers()
		.AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.DefaultIgnoreCondition =
					System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
		});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new()
	{
		Title = "Orders Service API",
		Version = "v1",
		Description = "E-Commerce Orders Microservice"
	});
});

builder.Services.AddOrdersServices(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks()
		.AddNpgSql(builder.Configuration.GetConnectionString("OrdersDb")!)
		.AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseOrdersMiddleware();
app.MapControllers();
app.MapHealthChecks("/health");

// Initialize database with retry logic
await RetryDatabaseInitializationAsync(app);

await app.RunAsync();


async Task RetryDatabaseInitializationAsync(WebApplication app, int maxRetries = 5)
{
	var logger = app.Services.GetRequiredService<ILogger<Program>>();

	for (int retry = 0; retry < maxRetries; retry++)
	{
		try
		{
			await app.InitializeDatabaseAsync();
			return;
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Database initialization attempt {Retry}/{MaxRetries} failed. Retrying in 5 seconds...",
					retry + 1, maxRetries);

			if (retry < maxRetries - 1)
			{
				await Task.Delay(TimeSpan.FromSeconds(5));
			}
			else
			{
				logger.LogError(ex, "Database initialization failed after {MaxRetries} attempts. Starting without database initialization.",
						maxRetries);
			}
		}
	}
}