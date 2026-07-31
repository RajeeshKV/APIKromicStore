using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using KromicStore.Infrastructure.Configuration;
using KromicStore.Infrastructure.Persistence;

namespace KromicStore.Infrastructure.Extensions;

/// <summary>
/// Extension methods for database operations during application startup.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations to the database during application startup.
    /// 
    /// This method:
    /// 1. Validates database configuration
    /// 2. Resolves the DbContext from the service provider
    /// 3. Detects pending migrations
    /// 4. Applies pending migrations with proper error handling
    /// 5. Logs every step for debugging and monitoring
    /// 
    /// Migration execution occurs asynchronously before the application begins serving requests.
    /// If migration fails and ContinueOnMigrationFailure is false, the application startup halts.
    /// </summary>
    /// <param name="serviceProvider">The IServiceProvider instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if database configuration is invalid or migrations cannot be applied
    /// (when ContinueOnMigrationFailure is false).
    /// </exception>
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IServiceCollection>>();
        
        try
        {
            // Get database configuration
            var options = scope.ServiceProvider.GetRequiredService<DatabaseOptions>();
            options.Validate();

            // Check if migrations should be applied
            if (!options.ApplyMigrationsOnStartup)
            {
                logger.LogInformation("Database migration on startup is disabled (ApplyMigrationsOnStartup=false)");
                return;
            }

            // Resolve DbContext
            var dbContext = scope.ServiceProvider.GetRequiredService<KromicStoreDbContext>();
            
            logger.LogInformation("Checking for pending database migrations...");

            // Get pending migrations
            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (!pendingMigrations.Any())
            {
                logger.LogInformation("Database is up-to-date. No pending migrations found.");
                return;
            }

            // Log pending migrations
            logger.LogInformation("Found {PendingMigrationCount} pending migration(s):", pendingMigrations.Count);
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation("  - {MigrationName}", migration);
            }

            // Apply migrations with timeout
            logger.LogInformation("Applying pending migrations with timeout of {TimeoutSeconds} seconds...", 
                options.MigrationTimeoutSeconds);

            using var cts = new System.Threading.CancellationTokenSource(
                TimeSpan.FromSeconds(options.MigrationTimeoutSeconds));

            await dbContext.Database.MigrateAsync(cts.Token);

            logger.LogInformation("All pending migrations applied successfully");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogCritical(ex, "Database migration timed out. This indicates a database connectivity issue or long-running migration.");
            if (!scope.ServiceProvider.GetRequiredService<DatabaseOptions>().ContinueOnMigrationFailure)
            {
                throw new InvalidOperationException(
                    "Database migration timed out and ContinueOnMigrationFailure is false. Application startup aborted.",
                    ex);
            }
            logger.LogWarning("Continuing startup despite migration timeout (ContinueOnMigrationFailure=true)");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to apply database migrations. This indicates a database schema or connectivity issue.");
            
            if (!scope.ServiceProvider.GetRequiredService<DatabaseOptions>().ContinueOnMigrationFailure)
            {
                throw new InvalidOperationException(
                    "Database migration failed and ContinueOnMigrationFailure is false. Application startup aborted.",
                    ex);
            }
            
            logger.LogWarning(ex, "Continuing startup despite migration failure (ContinueOnMigrationFailure=true)");
        }
    }

    /// <summary>
    /// Checks if the database connection is available without applying migrations.
    /// 
    /// Useful for health checks and diagnostics.
    /// </summary>
    /// <param name="serviceProvider">The IServiceProvider instance.</param>
    /// <returns>True if database is accessible; false otherwise.</returns>
    public static async Task<bool> CheckDatabaseConnectionAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IServiceCollection>>();
        
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<KromicStoreDbContext>();
            var canConnect = await dbContext.Database.CanConnectAsync();
            
            if (canConnect)
            {
                logger.LogInformation("Database connection successful");
            }
            else
            {
                logger.LogWarning("Database connection failed - CanConnectAsync returned false");
            }
            
            return canConnect;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking database connection");
            return false;
        }
    }
}
