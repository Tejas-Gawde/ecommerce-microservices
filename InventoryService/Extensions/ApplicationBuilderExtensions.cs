using InventoryService.Api.Middleware;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Extensions;

public static class ApplicationBuilderExtensions
{
  public static IApplicationBuilder UseInventoryMiddleware(this IApplicationBuilder app)
  {
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    return app;
  }

  public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
  {
    using var scope = app.ApplicationServices.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
      logger.LogInformation("Applying database migrations...");
      await dbContext.Database.MigrateAsync();
      logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while initializing the database");
      throw;
    }
  }
}