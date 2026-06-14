using CatalogService.Api.Middleware;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Extensions;

public static class ApplicationBuilderExtensions
{
  public static IApplicationBuilder UseCatalogMiddleware(this IApplicationBuilder app)
  {
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    return app;
  }

  public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
  {
    using var scope = app.ApplicationServices.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
      // Apply pending migrations first
      logger.LogInformation("Applying database migrations...");
      await dbContext.Database.MigrateAsync();
      logger.LogInformation("Database migrations applied successfully.");

      // Seed default categories if none exist
      if (!await dbContext.Categories.AnyAsync())
      {
        logger.LogInformation("Seeding default categories...");

        var categories = new[]
        {
                    Domain.Entities.Category.Create("Electronics"),
                    Domain.Entities.Category.Create("Clothing"),
                    Domain.Entities.Category.Create("Books"),
                    Domain.Entities.Category.Create("Home & Garden"),
                    Domain.Entities.Category.Create("Sports & Outdoors")
                };

        dbContext.Categories.AddRange(categories);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Default categories seeded successfully.");
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while initializing the database");
      throw;
    }
  }
}