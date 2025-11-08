using Microsoft.EntityFrameworkCore;
using RoomEnglish.Infrastructure.Data;

namespace RoomEnglish.Web.Middleware;

/// <summary>
/// Middleware to ensure database migrations are applied on first request
/// Use with caution in production
/// </summary>
public class MigrationMiddleware
{
    private readonly RequestDelegate _next;
    private static bool _migrationChecked = false;
    private static readonly object _lock = new object();

    public MigrationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, ILogger<MigrationMiddleware> logger)
    {
        if (!_migrationChecked)
        {
            lock (_lock)
            {
                if (!_migrationChecked)
                {
                    try
                    {
                        logger.LogInformation("Checking for pending migrations...");
                        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
                        
                        if (pendingMigrations.Any())
                        {
                            logger.LogWarning("Found {Count} pending migrations. Applying...", pendingMigrations.Count);
                            dbContext.Database.Migrate();
                            logger.LogInformation("Migrations applied successfully.");
                        }
                        else
                        {
                            logger.LogInformation("No pending migrations found.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error checking/applying migrations");
                        // Don't block the request
                    }
                    finally
                    {
                        _migrationChecked = true;
                    }
                }
            }
        }

        await _next(context);
    }
}

public static class MigrationMiddlewareExtensions
{
    public static IApplicationBuilder UseMigrationCheck(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MigrationMiddleware>();
    }
}
